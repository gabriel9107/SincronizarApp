using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;
using System.Data.SqlTypes;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;

namespace SigaApi
{
    class Program
    {



        static void Main(string[] args)
        {
            /*contexto 1 hace refrencia a la base de datos en sql para pedidos 
             * contexto 2 hace referencia a siga como base de datos 
             * */

            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"
            };


            /* se debe de obtener pedidos  (Listo)
             * se debe de evaluar si el pedido es de un nuevo cliente (Listo)
             * en caso de que sea un nuevo cliente, se debe de agregar el cliente a otra base de datos y el pedido tambien 
             * en caso de que el pedido sea de un cliente viejo, se debe de agregar directamente a ax 
             * se debe verificar los clientes modificados y actualizarlo a firebase 
             * se debe de sincronizar las facturas a firebase  (Estas facturas solo deben de estar las pendientes de pago) 
             * se debe de buscar los pagos realizados 
             * se de be de sincronizar los pagos a ax 
             * se debe de actualizar el estado del pago en ax 
             * se debe de actualizar el monto pendiente de la factura */



            obtenerPedido(ifc);

            //SincronizarFacturas(ifc);
            //SincronizarFacturaDetalles(ifc);


        }

        private static void obtenerPedido(IFirebaseConfig confi)
        {
            List<Pedido> pedidos = new List<Pedido>();


            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }

            var pedidoLista = new List<Pedido>();
            var respuesta = client.Get(@"Pedidos/");

            Dictionary<string, Pedido> obtenerPedidos = respuesta.ResultAs<Dictionary<string, Pedido>>();

            foreach (var get in obtenerPedidos)
            {

                pedidoLista.Add(new Pedido
                {
                    ClienteId = get.Value.ClienteId,
                    Compagnia = get.Value.Compagnia,
                    CreadoPor = get.Value.CreadoPor,
                    FechaOrden = get.Value.FechaOrden,
                    Impuestos = get.Value.Impuestos,
                    IsDelete = get.Value.IsDelete,
                    idFire = get.Key,
                    Sincronizado = get.Value.Sincronizado,
                    totalAPagar = get.Value.totalAPagar,
                });



            };
             
            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

            foreach (var pedido in pedidosAsincronizar)
            {

                AgregarPedidosSigaAdmin(pedido);

                Console.WriteLine(pedido.id);
            }




        }
        private static void AgregarPedidosSigaAdmin(Pedido pedido)
        {
            bool clienteNuevo = false;
            Customers clienteId;
            SigaAdminEntities context1 = new SigaAdminEntities();


            SigaAdminEntities context2 = new SigaAdminEntities();

            //var id = context2.Customers
            //       .Where(s => s.CustomerCode == pedido.ClienteId)
            //        .FirstOrDefault<Customer>();
            var cliente = context2.CUSTTABLEs.Where(s => s.Codigo == pedido.ClienteId).FirstOrDefault();




            if (cliente == null)
            {
                obtenerClientes();

                var existeCliente = context1
                    .Customers
                       .Where(s => s.CustomerCode == pedido.ClienteId)
                        .FirstOrDefault<Customers>();
                if (existeCliente != null)
                {
                    var t = new Invoices //Make sure you have a table called test in DB
                    {
                        Date = (DateTime)pedido.FechaOrden,
                        CustomerID = existeCliente.ID,
                        IDSaled = null,
                        ID = pedido.id,
                        Totals = pedido.totalAPagar,
                        VAT = pedido.Impuestos,
                        VendorCode = pedido.CreadoPor,
                        Status = 1,
                        IsNew = true,
                        IsInAx = false,
                        

                    };

                    context2.Invoices.Add(t);
                    context2.SaveChanges();

                    pedido.id = t.ID;
                    ActualizarPedidoFire(pedido);


                    obtenerPedidoDetalle(pedido, clienteNuevo);


                }




            }
            else
            {
                var idPedido = agregarpedidoAx(pedido.ClienteId);

                obtenerPedidoDetalleasync(idPedido, clienteNuevo);
            }



            //sincronizarPedidoAxAsync(t);

            ///ActualizarPedidoFire(pedido);

            //Console.WriteLine("Get Id " + t.ID);
        }

        private static void ActualizarPedidoFire(Pedido pedido)
        {
            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

            };
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client == null)
            {
                return; 
            }


            Pedido toUpdate = new Pedido()
            {
                Sincronizado = 0,
                ClienteId = pedido.ClienteId,
                Impuestos = pedido.Impuestos,
                NumeroOrden = pedido.NumeroOrden,
                totalAPagar = pedido.totalAPagar,
                FechaOrden = pedido.FechaOrden,
                IsDelete = pedido.IsDelete,
                CreadoPor = pedido.CreadoPor,
                Compagnia = pedido.Compagnia
            };


            var setter = client.Update("Pedidos/" + pedido.idFire, toUpdate);
            Console.WriteLine("Pedido Sincronizado" + pedido.id);
        }


        private static void agregarPedidoDetalleAx(PedidoDetalle detalle)
        {

            //descomentar 
            //SigaServicio.SGSIGAClient Client = new SigaServicio.SGSIGAClient();
            //Client.ClientCredentials.Windows.ClientCredential.Domain = "siga.local";
            //Client.ClientCredentials.Windows.ClientCredential.UserName = "axadmin";
            //Client.ClientCredentials.Windows.ClientCredential.Password = "Nomelose123";
            //SigaServicio.CallContext CallContext = new SigaServicio.CallContext();

            //Client.AddSaleLineOrder(CallContext, orderId, productoId, precio, cantidad);

            using(var context = new SigaAdminEntities())
            {
                var st = new PedidoDetalleAx()
                {
                    PedidoSigaId = detalle.Nombre,
                    Cantidad = detalle.Cantidad,
                    Codigo = detalle.Codigo,
                    Precio = detalle.Precio
                };

                context.PedidoDetalleAxes.Add(st);
                context.SaveChanges(); 
            }



        }


        private static string agregarpedidoAx(string cliente)
        {
            using (var context = new SigaAdminEntities())
            {
                var std = new pedidoAx()
                {
                    clienteNuevo = cliente
                };
                context.pedidoAxes.Add(std);
                context.SaveChanges();

                return std.id.ToString(); 
            }
            
                    
        }

        private static void obtenerPedidoDetalleasync(string PedidoId, bool clienteNuevo)
        {
            List<PedidoDetalle> pedidos = new List<PedidoDetalle>();
            IFirebaseConfig confi = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"
            };


            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }

            var pedidoLista = new List<PedidoDetalle>();
            var respuesta = client.Get(@"PedidoDetalle/");

            Dictionary<string, PedidoDetalle> obtenerPedidos = respuesta.ResultAs<Dictionary<string, PedidoDetalle>>();

            foreach (var get in obtenerPedidos)
            {

                pedidoLista.Add(new PedidoDetalle
                {
                    Cantidad = get.Value.Cantidad,
                    Codigo = get.Value.Codigo,
                    Precio = get.Value.Precio,
                    Id = get.Key
                });


                //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
                //pedidoLista.Add(get.Value);



            };

            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

            foreach (var detalle in pedidosAsincronizar)
            {

                //agregarPedidoDetalleAx(PedidoId, detalle.Codigo, detalle.Precio, detalle.Cantidad);

            }
        }



        private static void obtenerPedidoDetalle(Pedido pedido, bool clienteNuevo)
        {
            List<PedidoDetalle> pedidos = new List<PedidoDetalle>();
            IFirebaseConfig confi = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"
            };


            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }

            var pedidoLista = new List<PedidoDetalle>();
            var respuesta = client.Get(@"PedidoDetalle/");

            Dictionary<string, PedidoDetalle> obtenerPedidos = respuesta.ResultAs<Dictionary<string, PedidoDetalle>>();

            foreach (var get in obtenerPedidos)
            {

                pedidoLista.Add(new PedidoDetalle
                {
                    Cantidad = get.Value.Cantidad,
                    Codigo = get.Value.Codigo,
                    Compagnia = get.Value.Compagnia,
                    Nombre = get.Value.Nombre,
                    IsDelete = get.Value.IsDelete,
                    PedidoId = pedido.id,
                    Precio = get.Value.Precio,
                    ProductoId = get.Value.ProductoId,
                    Sincronizado = get.Value.Sincronizado,
                    IdFire = get.Key,
                    idfirebase = get.Value.idfirebase
                }) ;

                 

            };

            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1 && x.idfirebase == pedido.idFire);

            foreach (var detalle in pedidosAsincronizar)
            {

                AgregarPedidoDetalleSigaAdmin(detalle, pedido.id);

            }
        }
        private static void AgregarPedidoDetalleSigaAdmin(PedidoDetalle pedido, int pedidoid)
        {
            SigaAdminEntities context2 = new SigaAdminEntities();



            var t = new InvoiceLines 
            {
                Qty = pedido.Cantidad,
                InvoiceID = pedido.PedidoId, 
                
                
                Price = pedido.Precio,
                ProductCode = pedido.Codigo,



                IsNew = true,
                IsInAx = false



            };
            


                context2.InvoiceLines.Add(t);
                context2.SaveChanges();

            ActualizarPedidoDetalleFire(pedido);


              
        }

        private static void ActualizarPedidoDetalleFire(PedidoDetalle detalle)
        {
            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

            };
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }


            PedidoDetalle toUpdate = new PedidoDetalle()
            {               
                Cantidad = detalle.Cantidad, 
                Codigo = detalle.Codigo, 
                Compagnia = detalle.Compagnia, 
                Nombre = detalle.Nombre, 
                IsDelete = detalle.IsDelete , 
                PedidoId = detalle.PedidoId,
                Precio = detalle.Precio, 
                Sincronizado = 0, 
                idfirebase = detalle.idfirebase
            };


            var setter = client.Update("PedidoDetalle/" + detalle.IdFire, toUpdate);
            Console.WriteLine("Pedido Sincronizado" + detalle.IdFire);

        }





        /*Clientes */


        private static void obtenerClientes()
        {

            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"
            };

            //Lista de Clientes 
            List<Clientes> clientes = new List<Clientes>();


            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }

            var clientesList = new List<Clientes>();
            var respuesta = client.Get(@"Clientes/");

            Dictionary<string, Clientes> obtenerClientes = respuesta.ResultAs<Dictionary<string, Clientes>>();

            foreach (var get in obtenerClientes)
            {
                clientesList.Add(new Clientes
                {
                    ID = get.Key,
                    activo = get.Value.activo,
                    codigo = get.Value.codigo,
                    codigoVendedor = get.Value.codigoVendedor,
                    descuento = get.Value.descuento,
                    comentario = get.Value.comentario,
                    compagnia = get.Value.compagnia,
                    direccion = get.Value.direccion,
                    nombre = get.Value.nombre,
                    sincronizado = get.Value.sincronizado,
                    telefono1 = get.Value.telefono1,
                    telefono2 = get.Value.telefono2
                });


            };

            var ClienteASincronizar = clientesList.Where(x => x.sincronizado == 1);

            foreach (var cliente in ClienteASincronizar)
            {

                AgregarClienteSigaAdmin(cliente);

                Console.WriteLine(cliente.nombre);
            }

            Console.WriteLine("CLientes sincronizados");
            

        }
        private static void AgregarClienteSigaAdmin(Clientes cliente)
        {

            SigaAdminEntities context = new SigaAdminEntities();
            //SigaAppEntities context 

            var t = new Customers //Make sure you have a table called test in DB
            {
                CustomerName = cliente.nombre,
                CustomerCode = cliente.codigo,
                CustomerDir = cliente.direccion,
                Phone1 = cliente.telefono1,
                Phone2 = cliente.telefono2,
                ShippingMth = cliente.descuento,
                VendorCode = cliente.codigoVendedor,
                Comment1 = cliente.comentario,
                IsNew = true,
                IsInAx = false
            };
            context.Customers.Add(t);
            context.SaveChanges();

            ActualizarCliente(cliente);
        }
        private static void ActualizarCliente(Clientes clientes)
        {
            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

            };
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }


            Clientes toUpdate = new Clientes()
            {
                codigo = clientes.codigo,
                codigoVendedor = clientes.codigoVendedor,
                direccion = clientes.direccion,
                activo = clientes.activo,
                nombre = clientes.nombre,
                comentario = clientes.comentario,
                telefono1 = clientes.telefono1,
                telefono2 = clientes.telefono2,
                sincronizado = 0
            };


            var setter = client.Update("Clientes/" + clientes.ID, toUpdate);
            Console.WriteLine("sincronizado");
        }
    }

}



//        private static void obtenerPedido(IFirebaseConfig confi)
//        {
//            List<Pedido> pedidos = new List<Pedido>();


//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var pedidoLista = new List<Pedido>();
//            var respuesta = client.Get(@"Pedidos/");

//            Dictionary<string, Pedido> obtenerPedidos = respuesta.ResultAs<Dictionary<string, Pedido>>();

//            foreach (var get in obtenerPedidos)
//            {

//                pedidoLista.Add(new Pedido
//                {


//                    ClienteId = get.Value.ClienteId,
//                    Compagnia = get.Value.Compagnia,
//                    CreadoPor = get.Value.CreadoPor,
//                    FechaOrden = get.Value.FechaOrden,
//                    Impuestos = get.Value.Impuestos,
//                    IsDelete = get.Value.IsDelete,
//                    //id = get.Value.
//                    //NumeroOrden = int.Parse(get.Value.id),
//                    Sincronizado = get.Value.Sincronizado,
//                    totalAPagar = get.Value.totalAPagar,
//                    id = get.Key


//                });



//            };

//            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

//            foreach (var pedido in pedidosAsincronizar)
//            {

//                AgregarPedidosSigaAdmin(pedido);

//                Console.WriteLine(pedido.id);
//            }




//        }



//        private static void SincronizarCLientes(IFirebaseConfig confi)
//        {
//            List<Clientes> clientes  = new List<Clientes>(); 

//            SigaAdminEntities context = new SigaAdminEntities();

//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }
//            var _customers = from a in context.Customers
//                            where a.subir == 1
//                            select a;
//            foreach(Customers customers
//                in _customers)
//            {
//                clientes.Add(new Clientes
//                {
//                    activo = 0,
//                    codigo = customers.CustomerCode,
//                    codigoVendedor = customers.VendorCode,
//                    comentario = customers.Comment1,
//                    compagnia = 0,
//                    descuento = customers.ShippingMth,
//                    direccion = customers.CustomerDir,
//                    nombre = customers.CustomerName,
//                    telefono1 = customers.Phone1,
//                    telefono2 = customers.Phone2,
//                    sincronizado = 0,
//                    ID = "0"

//                });

//            }
            

//            foreach (Clientes customer in clientes)
//            {
//                client.Set("Clientes/" + customer.codigo, customer);
//                Console.WriteLine(customer.nombre);
//            }

//            Console.WriteLine();
//            Console.ReadLine();
//        }



 

//        private static void obtenerClientes(IFirebaseConfig confi)
//        {
//            //Lista de Clientes 
//            List<Clientes> clientes = new List<Clientes>();


//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var clientesList = new List<Clientes>();
//            var respuesta = client.Get(@"Clientes/");

//            Dictionary<string, Clientes> obtenerClientes = respuesta.ResultAs<Dictionary<string, Clientes>>();

//            foreach (var get in obtenerClientes)
//            {
//                clientesList.Add(new Clientes
//                {
//                    ID = get.Key,
//                    activo = get.Value.activo,
//                    codigo = get.Value.codigo,
//                    codigoVendedor = get.Value.codigoVendedor,
//                    comentario = get.Value.comentario,
//                    compagnia = get.Value.compagnia,
//                    direccion = get.Value.direccion,
//                    nombre = get.Value.nombre,
//                    sincronizado = get.Value.sincronizado,
//                    telefono1 = get.Value.telefono1,
//                    telefono2 = get.Value.telefono2
//                });
//                //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
//                //clientesList.Add(get.Value);


//            };

//            var ClienteASincronizar = clientesList.Where(x => x.sincronizado == 1);

//            foreach (var cliente in ClienteASincronizar)
//            {

//                AgregarClienteSigaAdmin(cliente);

//                Console.WriteLine(cliente.nombre);
//            }


//            Console.WriteLine("Clientes Listos");

//        }
//        private static void AgregarClienteSigaAdmin(Clientes cliente)
//        {

//            SigaAdminEntities context = new SigaAdminEntities();
//            //SigaAppEntities context 

//            var t = new Customers //Make sure you have a table called test in DB
//            {
//                idFire = cliente.ID,
//                CustomerName = cliente.nombre,
//                CustomerCode = cliente.codigo,
//                CustomerDir = cliente.direccion,
//                Phone1 = cliente.telefono1,
//                Phone2 = cliente.telefono2,
//                ShippingMth = cliente.descuento,
//                VendorCode = cliente.codigoVendedor,
//                Comment1 = cliente.comentario,
//                IsNew = true,
//                IsInAx = false
//            };
//            context.Customers.Add(t);
//            context.SaveChanges();

//            ActualizarCliente(cliente);
//        }
//        private static void ActualizarCliente(Clientes clientes)
//        {
//            IFirebaseConfig ifc = new FirebaseConfig()
//            {
//                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
//                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

//            };
//            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }


//            Clientes toUpdate = new Clientes()
//            {
//                codigo = clientes.codigo,
//                codigoVendedor = clientes.codigoVendedor,
//                direccion = clientes.direccion,
//                activo = clientes.activo,
//                nombre = clientes.nombre,
//                comentario = clientes.comentario,
//                telefono1 = clientes.telefono1,
//                telefono2 = clientes.telefono2,
//                sincronizado = 0
//            };


//            var setter = client.Update("Clientes/" + clientes.ID, toUpdate);
//            Console.WriteLine("sincronizado");
//        }




//        private static void obtenerPedido(IFirebaseConfig confi)
//        {
//            List<Pedido> pedidos = new List<Pedido>();


//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var pedidoLista = new List<Pedido>();
//            var respuesta = client.Get(@"Pedidos/");

//            Dictionary<string, Pedido> obtenerPedidos = respuesta.ResultAs<Dictionary<string, Pedido>>();

//            foreach (var get in obtenerPedidos)
//            {

//                pedidoLista.Add(new Pedido
//                {
                    
//                    ClienteId = get.Value.ClienteId,
//                    Compagnia = get.Value.Compagnia,
//                    CreadoPor = get.Value.CreadoPor,
//                    FechaOrden = get.Value.FechaOrden,
//                    Impuestos = get.Value.Impuestos,
//                    IsDelete = get.Value.IsDelete,
                    
//                    NumeroOrden = get.Value.NumeroOrden,
//                    Sincronizado = get.Value.Sincronizado,
//                    totalAPagar = get.Value.totalAPagar,
//                    id = get.Value.id, 
//                    ifFire = get.Key
                   
//                });

//            };

//            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

//            foreach (var pedido in pedidosAsincronizar)
//            {

//                AgregarPedidosSigaAdmin(pedido);

//                Console.WriteLine(pedido.id);
//            }

//        }

//        private static void BuscarClienteEnEspecifico(string id )
//        {
//            IFirebaseConfig ifc = new FirebaseConfig()
//            {
//                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
//                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

//            };

//            List<Clientes> clientes = new List<Clientes>();


//            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var clientesList = new List<Clientes>();
//            var respuesta = client.Get(@"Clientes/");

//            Dictionary<string, Clientes> obtenerClientes = respuesta.ResultAs<Dictionary<string, Clientes>>();

//            foreach (var get in obtenerClientes)
//            {
//                clientesList.Add(new Clientes
//                {
//                    ID = get.Key,
//                    activo = get.Value.activo,
//                    codigo = get.Value.codigo,
//                    codigoVendedor = get.Value.codigoVendedor,
//                    comentario = get.Value.comentario,
//                    compagnia = get.Value.compagnia,
//                    direccion = get.Value.direccion,
//                    nombre = get.Value.nombre,
//                    sincronizado = get.Value.sincronizado,
//                    telefono1 = get.Value.telefono1,
//                    telefono2 = get.Value.telefono2
//                });
//                //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
//                //clientesList.Add(get.Value);


//            };

//            var ClienteASincronizar = clientesList.Where(x => x.codigo == id);

//            foreach (var cliente in ClienteASincronizar)
//            {

//                AgregarClienteSigaAdmin(cliente);

//                Console.WriteLine(cliente.nombre);
//            }

//        }

//        private static void AgregarPedidosSigaAdmin(Pedido pedido)
//        {
//            SigaAdminEntities context2 = new SigaAdminEntities();


//            var id = context2.Customers
//                   .Where(s => s.CustomerCode == pedido.ClienteId)
//                    .FirstOrDefault<Customers>();

//            if(id == null )
//            {
//                BuscarClienteEnEspecifico(pedido.ClienteId);
//                  id = context2.Customers
//                .Where(s => s.CustomerCode == pedido.ClienteId)
//                 .FirstOrDefault<Customers>();
//                if(id == null)
//                { return; }

//            }



//            var t = new Invoices //Make sure you have a table called test in DB
//            {
//                Date = (DateTime)pedido.FechaOrden,
//                CustomerID = id.ID,
//                ifFire = pedido.ifFire,
//                //IDSaled = pedido.NumeroOrden
//                //CustomerID = pedido.ClienteId,
//                Totals = pedido.totalAPagar,
//                VAT = pedido.Impuestos,
//                VendorCode = pedido.CreadoPor,
//                Status = 1,
//                IsNew = true,
//                IsInAx = false



//            };
//            context2.Invoices.Add(t);
//            context2.SaveChanges();


//            ActualizarPedidoFire(pedido);
//            obtenerPedidoDetalle(t);
//            //Console.WriteLine("Get Id " + t.ID);
//        }

//        private static void obtenerPedidoDetalle(Invoices pedido)
//        {
//            List<PedidoDetalle> pedidos = new List<PedidoDetalle>();
//            IFirebaseConfig confi = new FirebaseConfig()
//            {
//                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
//                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"
//            };


//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var pedidoLista = new List<PedidoDetalle>();
//            var respuesta = client.Get(@"PedidoDetalle/");

//            Dictionary<string, PedidoDetalle> obtenerPedidos = respuesta.ResultAs<Dictionary<string, PedidoDetalle>>();

//            foreach (var get in obtenerPedidos)
//            {

//                pedidoLista.Add(new PedidoDetalle
//                {
//                    Cantidad = get.Value.Cantidad,
//                    Codigo = get.Value.Codigo,
//                    Compagnia = get.Value.Compagnia,
//                    Nombre = get.Value.Nombre,
//                    IsDelete = get.Value.IsDelete,
//                    PedidoId = pedido.ID,
//                    Precio = get.Value.Precio,
//                    ProductoId = get.Value.ProductoId,
//                    Sincronizado = get.Value.Sincronizado,
//                    Id = get.Key
//                });


//                //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
//                //pedidoLista.Add(get.Value);



//            };

//            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

//            foreach (var detalle in pedidosAsincronizar)
//            {

//                AgregarPedidoDetalleSigaAdmin(detalle, pedido.ID);

//            }
//        }
//        private static void AgregarPedidoDetalleSigaAdmin(PedidoDetalle pedido, int pedidoid)
//        {
//            SigaAdminEntities context2 = new SigaAdminEntities();


//            //var idProductoId = context2.
//            //       .Where(s => s.CustomerCode == pedido.ClienteId)
//            //        .FirstOrDefault<Customer>();


//            var t = new InvoiceLines //Make sure you have a table called test in DB
//            {
//                Qty = pedido.Cantidad,
//                InvoiceID = pedido.PedidoId,
//                Price = pedido.Precio,
//                ProductCode = pedido.Codigo,



//                IsNew = true,
//                IsInAx = false



//            };
//            context2.InvoiceLines.Add(t);
//            context2.SaveChanges();

//            ActualizarPedidoDetalleFire(pedido);
//        }
//        private static void ActualizarPedidoDetalleFire(PedidoDetalle detalle)
//        {
//            IFirebaseConfig ifc = new FirebaseConfig()
//            {
//                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
//                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

//            };
//            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }


//            Pedido toUpdate = new Pedido()
//            {
//                Sincronizado = 0
//            };


//            var setter = client.Update("PedidoDetalle/" + detalle.Id, toUpdate);
//            Console.WriteLine("Pedido Sincronizado" + detalle.PedidoId);

//        }


//        private static void ActualizarPedidoFire(Pedido pedido)
//        {
//            IFirebaseConfig ifc = new FirebaseConfig()
//            {
//                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
//                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

//            };
//            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }


//            Pedido toUpdate = new Pedido()
//            {NumeroOrden = pedido.NumeroOrden, 
//            //ifFire = pedido.ifFire, 
//            ClienteId = pedido.ClienteId, 
//            Impuestos = pedido.Impuestos, 
//            FechaOrden=  pedido.FechaOrden, 
//            //id = pedido.id, 
//            Compagnia = pedido.Compagnia, 
//            CreadoPor = pedido.CreadoPor, 
//            IsDelete = pedido.IsDelete , 
//            totalAPagar = pedido.totalAPagar,
//              Sincronizado = 0
//            };


//            var setter = client.Update("Pedidos/" + pedido.ifFire, toUpdate);
//            Console.WriteLine("Pedido Sincronizado" + pedido.id);
//        }
//        private static void SincronizarFacturas(IFirebaseConfig confi)
//        {
//            SigaAdminEntities context2 = new SigaAdminEntities();
//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var facturas = from a in context2.Facturas
//                           where a.Sincronizado == 1
//                           select a;

//            foreach (Factura factura in facturas)
//            {
//                client.Set("Factura/" + factura.FacturaId, factura);
//                updateFacturas(factura.FacturaId);

//            }
//            Console.WriteLine("Sincronizado");
//            SincronizarFacturaDetalles(confi);
//        }
//        private static void updateFacturas(string facturaid)
//        {
//            using (var context = new SigaAdminEntities())
//            {
//                var resultado = context.Facturas.SingleOrDefault(b => b.FacturaId == facturaid);
//                if (resultado != null)
//                {
//                    resultado.Sincronizado = 0;
//                    context.SaveChanges();
//                }
//            }

//        }

//        private static void SincronizarFacturaDetalles(IFirebaseConfig confi)
//        {
//            SigaAdminEntities context2 = new SigaAdminEntities();
//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var detalles = from a in context2.FacturaDetalles
//                           where a.Sincronizado == 1
//                           select a;

//            foreach (FacturaDetalle detalle in detalles)
//            {
//                client.Set("FacturaDetalle/" + detalle.ID+'-'+ detalle.FacturaId, detalle);
//                updateFacturaDetalles(detalle.ID);

//            }
//            Console.WriteLine("Sincronizado");
//        }
//        private static void updateFacturaDetalles(int id)
//        {
//            using (var context = new SigaAdminEntities())
//            {
//                var resultado = context.FacturaDetalles.SingleOrDefault(b => b.ID == id);
//                if (resultado != null)
//                {
//                    resultado.Sincronizado = 0;
//                    context.SaveChanges();
//                }
//            }

//        }
//        private static void obtenerPago(IFirebaseConfig confi)
//        {
//            //Lista de Clientes 
//            List<Pago> pago = new List<Pago>();


//            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }

//            var pagoList = new List<Pago>();
//            var respuesta = client.Get(@"Pago/");

//            Dictionary<string, Pago> obtenerPagos = respuesta.ResultAs<Dictionary<string, Pago>>();

//            foreach (var get in obtenerPagos)
//            {
//                pagoList.Add(new Pago
//                {

//                    ID = get.Key,
//                    Banco = get.Value.Banco, 
                   
//                    ClienteId = get.Value.ClienteId, 
//                    Compagni = get.Value.Compagni, 
//                    FechaDeCheque = get.Value.FechaDeCheque , 
//                    FechaPago = get.Value.FechaPago , 
//                    IsDelete = get.Value.IsDelete, 
//                    MetodoDePago = get.Value.MetodoDePago, 
//                    MontoPagado = get.Value.MontoPagado, 
//                    NumeroDeCheque = get.Value.NumeroDeCheque, 
//                    Pendiente = get.Value.Pendiente, 
//                    Sincronizado = get.Value.Sincronizado, 
//                    VendorId = get.Value.VendorId
                    
//                });  
                


//            };

//            var pagosAsincronizar = pagoList.Where(x => x.Sincronizado == 1);

//            foreach (var pagos in pagosAsincronizar)
//            {

//                agregarApagos(pagos);

//                Console.WriteLine(pagos.ClienteId);
//            }


//        }
//        private static void agregarApagos(Pago pago)
//        {

//            SigaAdminEntities context = new SigaAdminEntities();
//            //SigaAppEntities context 

//            var t = new PaymentOrder //Make sure you have a table called test in DB
//            {
//              VendorID = pago.VendorId, 
//              CheckDate= DateTime.Now,
//                Datetime = DateTime.Now,  
//              Amount = pago.MontoPagado, 
//              Method = pago.MetodoDePago, 
//              BankName = pago.Banco, 
//              CheckNumber = pago.NumeroDeCheque, 
//              IsEnabled = true, 
//              Customer_Code = pago.ClienteId, 
//              IsOpen = true, 
//              Imported = false
               
               
//            };
//            context.PaymentOrders.Add(t);
//            context.SaveChanges();

//            //ActualizarCliente(pago);
//        }
//        private static void ActualizarPagoFire(Pago detalle)
//        {
//            IFirebaseConfig ifc = new FirebaseConfig()
//            {
//                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
//                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

//            };
//            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
//            if (client != null)
//            {
//                Console.WriteLine("connection esta estabilizada");
//            }


//            Pago toUpdate = new Pago()
//            {
//                Sincronizado = 0
//            };


//            var setter = client.Update("Pago/" + detalle.ID, toUpdate);
//            Console.WriteLine("Pago Sincronizado" + detalle.ID);

//        }


//    }

     


//}

////private static void obtenerPedidoDetalle(Invoice pedido)
////{
////    List<PedidoDetalle> pedidos = new List<PedidoDetalle>();
////    IFirebaseConfig confi = new FirebaseConfig()
////    {
////        AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
////        BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"
////    };


////    IFirebaseClient client = new FireSharp.FirebaseClient(confi);
////    if (client != null)
////    {
////        Console.WriteLine("connection esta estabilizada");
////    }

////    var pedidoLista = new List<PedidoDetalle>();
////    var respuesta = client.Get(@"PedidoDetalle/");

////    Dictionary<string, PedidoDetalle> obtenerPedidos = respuesta.ResultAs<Dictionary<string, PedidoDetalle>>();

////    foreach (var get in obtenerPedidos)
////    {



////        //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
////        pedidoLista.Add(get.Value);



////    };

////    var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

////    foreach (var detalle in pedidosAsincronizar)
////    {

////        AgregarPedidoDetalleSigaAdmin(detalle, pedido.ID);

////    }





////    Console.ReadLine();
////    Console.Read();
////}
////private static void AgregarPedidoDetalleSigaAdmin(PedidoDetalle pedido, int pedidoid)
////{
////    SigaAdminEntities context2 = new SigaAdminEntities();


////    //var idProductoId = context2.
////    //       .Where(s => s.CustomerCode == pedido.ClienteId)
////    //        .FirstOrDefault<Customer>();


////    var t = new InvoiceLine //Make sure you have a table called test in DB
////    {
////        Qty = pedido.Cantidad, 
////        InvoiceID = pedidoid, 
////        Price = pedido.Precio, 
////        ProductCode = pedido.Codigo, 



////        IsNew = true,
////        IsInAx = false



////    };
////    context2.InvoiceLines.Add(t);
////    context2.SaveChanges();

////    ActualizarPedidoDetalleFire(pedido);
////}
////private static void AgregarPedidosSigaAdmin(Pedido pedido)
////{
////    SigaAdminEntities context2 = new SigaAdminEntities();


////    var id = context2.Customers
////           .Where(s => s.CustomerCode == pedido.ClienteId)
////            .FirstOrDefault<Customer>();


////    var t = new Invoice //Make sure you have a table called test in DB
////    {
////        Date = (DateTime)pedido.FechaOrden,
////        CustomerID = id.ID,
////        //CustomerID = pedido.ClienteId,
////        Totals = pedido.totalAPagar,
////        VAT = pedido.Impuestos,
////        VendorCode = pedido.CreadoPor,
////        Status = 1,
////        IsNew = true,
////        IsInAx = false



////    };
////    context2.Invoices.Add(t);
////    context2.SaveChanges();


////    obtenerPedidoDetalle(t);
////    //Console.WriteLine("Get Id " + t.ID);
////    //ActualizarPedidoFire(pedido);
////}


////private static void ActualizarPedidoDetalleFire(PedidoDetalle detalle)
////    {
////        IFirebaseConfig ifc = new FirebaseConfig()
////        {
////            AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
////            BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"

////        };
////        IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
////        if (client != null)
////        {
////            Console.WriteLine("connection esta estabilizada");
////        }


////        Pedido toUpdate = new Pedido()
////        {
////            Sincronizado = 0
////        };


////        var setter = client.Update("PedidoDetalle/" + detalle.PedidoId, toUpdate);
////        Console.WriteLine("Pedido Sincronizado" + detalle.PedidoId);

////    }



////private static void SincronizarFacturaDetalles(IFirebaseConfig confi)
////{
////    SigaAdminEntities context2 = new SigaAdminEntities();
////    IFirebaseClient client = new FireSharp.FirebaseClient(confi);
////    if (client != null)
////    {
////        Console.WriteLine("connection esta estabilizada");
////    }

////    var detalles = from a in context2.FacturaDetalles
////                   where a.Sincronizado == 1
////                   select a;

////    foreach (FacturaDetalle factura in detalles)
////    {
////        client.Set("FacturaDetalle/" + factura.ID, factura);
////        updateFacturas(factura.FacturaId);

////    }
////    Console.WriteLine("Sincronizado");
////}

////private static void updateFacturaDetalles(string facturaid)
////{
////    using (var context = new SigaAdminEntities())
////    {
////        var resultado = context.FacturaDetalles.SingleOrDefault(b => b.FacturaId == facturaid);
////        if (resultado != null)
////        {
////            resultado.Sincronizado = 0;
////            context.SaveChanges();
////        }
////    }
////}





