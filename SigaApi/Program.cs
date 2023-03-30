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
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"
            };



            /* se debe de obtener pedidos  (Listo)
             * se debe de evaluar si el pedido es de un nuevo cliente (Listo)
             * en caso de que sea un nuevo cliente, se debe de agregar el cliente a otra base de datos y el pedido tambien (Listo)
             * en caso de que el pedido sea de un cliente viejo, se debe de agregar directamente a ax 
             * se debe verificar los clientes modificados y actualizarlo a firebase (Listo)
             * se debe de sincronizar las facturas a firebase  (Estas facturas solo deben de estar las pendientes de pago) (Listo)
             * se debe de buscar los pagos realizados 
             * se de be de sincronizar los pagos a ax 
             * se debe de actualizar el estado del pago en ax 
             * se debe de actualizar el monto pendiente de la factura */

            //1-) Obtener Pedido (Se sincronizaran los pedidos, en caso de que el cliente sea nuevo, se  van a sincronizar los clientes nuevo y sus pedidos )
            obtenerPedido(ifc);

            //2 - )
            //Obtiene la lista de clientes que fueron modificados en Siga y se deben de actualizar en los clientes
            ObtenerClientesModificados(ifc);
            //3-)
            // Obtener Listado de facturas pendiente de sincronizar 
            SincronizarFacturas(ifc);

            //Buscando los pagos pendiente de sincronizar 
            obtenerPago(ifc);

            //Buscando los productos pendientes
            sincronizarProdutos( );




            //Agregar los pagos pendiente a ax 

            //Sincronizar los pagos de AX a Intermedia e intermedia a firebase 


            //Sincronizando el monto pendiente de pago en firebase 








        }

        private static void sincronizarProdutos(   )
        {

            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"
            };

            SigaAdminEntities context2 = new SigaAdminEntities();
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client == null)
            {
                return;
            }

            var productos = from a in context2.productos
                            where a.Sincronizado == 1
                            select a;

            foreach (producto product in productos)
            {
                client.Set("Productos/" + product.Id, product);

                updateProducts(product.Codigo);
            }


        }

        private static void ActualizarProductosPendienteDeCambios(IFirebaseConfig ifc)
        {
            SigaAdminEntities context2 = new SigaAdminEntities();
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client == null)
            {
                return;
            }

            var productos = from a in context2.productos
                            where a.actualizar == 1
                            select a;

            foreach (producto product in productos)
            {
                //var setter = client.Update("Pedidos/" + pedido.idFire, toUpdate);
                client.Update("Productos/" + product.Codigo, product);

                updateproductosModificados(product.Codigo);
            }

        }

        private static void updateproductosModificados(string codigoId)
        {
            using (var context = new SigaAdminEntities())
            {
                var resultado = context.productos.SingleOrDefault(b => b.Codigo == codigoId);
                if (resultado != null)
                {
                    resultado.actualizar = 0;
                    context.SaveChanges();
                }
            }

        }

        private static void updateProducts(string codigoId)
        {
            using (var context = new SigaAdminEntities())
            {
                var resultado = context.productos.SingleOrDefault(b => b.Codigo == codigoId);
                if (resultado != null)
                {
                    resultado.actualizar = 0;
                    context.SaveChanges();
                }
            }

        }

        //Productos 




        //List<producto> productos = new List<producto>();


        //IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
        //if (client == null)
        //{
        //    return;

        //}

        //var pedidoLista = new List<Pedido>();
        //var respuesta = client.Get(@"Prod/");

        //Dictionary<string, producto> obtenerProductos = respuesta.ResultAs<Dictionary<string, producto>>();

        //foreach (var get in obtenerProductos)
        //{

        //    pedidoLista.Add(new Pedido
        //    {
        //        Id = get.Value.Id,
        //        ClienteId = get.Value.ClienteId,
        //        Compagnia = get.Value.Compagnia,
        //        CreadoPor = get.Value.CreadoPor,
        //        FechaOrden = get.Value.FechaOrden,
        //        Impuestos = get.Value.Impuestos,
        //        IsDelete = get.Value.IsDelete,
        //        idFire = get.Key,
        //        Sincronizado = get.Value.Sincronizado,
        //        totalAPagar = get.Value.totalAPagar,
        //    });



        //};

        //var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

        //foreach (var pedido in pedidosAsincronizar)
        //{

        //    AgregarPedidosSigaAdmin(pedido);

        //    Console.WriteLine(pedido.Id);
        //}


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
                    Id = get.Value.Id, 
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

                Console.WriteLine(pedido.Id);
            }




        }
        private static void AgregarPedidosSigaAdmin(Pedido pedido)
        {
            bool clienteNuevo = false;
            
            SigaAdminEntities context1 = new SigaAdminEntities();


            GRUPOSIGAXEntities contexto = new GRUPOSIGAXEntities();

            //var id = context2.Customers
            //       .Where(s => s.CustomerCode == pedido.ClienteId)
            //        .FirstOrDefault<Customer>();


            var cliente = contexto.CUSTTABLEs.Where(c => c.ACCOUNTNUM == pedido.ClienteId).FirstOrDefault<CUSTTABLE>();




            if (cliente == null)
            {
                obtenerClientes();

                var existeCliente = context1.Customers.Where(c => c.CustomerCode == pedido.ClienteId).FirstOrDefault<Customer>();
                if (existeCliente != null)
                {
                    var t = new Invoice 
                    {
                        Date = (DateTime)pedido.FechaOrden,
                        CustomerID = existeCliente.ID,
                        IDSaled = null,
                        ID = pedido.Id,
                        Totals = pedido.totalAPagar,
                        VAT = pedido.Impuestos,
                        VendorCode = pedido.CreadoPor,
                        Status = 1,
                        IsNew = true,
                        IsInAx = false,
                        

                    };

                    context1.Invoices.Add(t);
                    context1.SaveChanges();

                    pedido.Id = t.ID;
                    ActualizarPedidoFire(pedido);


                    obtenerPedidoDetalle(pedido, clienteNuevo);


                }




            }
            else
            {
                var idPedido = agregarpedidoAx(pedido.ClienteId);

                obtenerPedidoDetalleasync(idPedido, pedido.idFire);
            }



            //sincronizarPedidoAxAsync(t);

            ActualizarPedidoFire(pedido);

            //Console.WriteLine("Get Id " + t.ID);
        }

        private static void ActualizarPedidoFire(Pedido pedido)
        {
            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"

            };
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client == null)
            {
                return; 
            }


            Pedido toUpdate = new Pedido()
            {
                Id = pedido.Id,
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
            Console.WriteLine("Pedido Sincronizado" + pedido.Id);
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
                    PedidoSigaId = detalle.PedidoId.ToString(),
                    Cantidad = detalle.Cantidad,
                    Codigo = detalle.Codigo,
                    Precio = detalle.Precio
                };

                context.PedidoDetalleAxes.Add(st);
                context.SaveChanges(); 
            }



        }

        private static void obtenerPedidoDetalleasync(int IdPedido, string _idFirebase)
        {
            List<PedidoDetalle> pedidos = new List<PedidoDetalle>();
            IFirebaseConfig confi = new FirebaseConfig()
            {
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"
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
                    idfirebase = get.Value.idfirebase,
                    Cantidad = get.Value.Cantidad,
                    Codigo = get.Value.Codigo,
                    Precio = get.Value.Precio,
                    Id = get.Key
                });

            };

            var pedidosAsincronizar = pedidoLista.Where(x =>    x.idfirebase == _idFirebase);

            foreach (var detalle in pedidosAsincronizar)
            {
                detalle.PedidoId = IdPedido;

                agregarPedidoDetalleAx(detalle);

            }
        }

        private static int agregarpedidoAx(string cliente)
        {
            using (var context = new SigaAdminEntities())
            {
                var std = new pedidoAx()
                {
                    clienteNuevo = cliente
                };
                context.pedidoAxes.Add(std);
                context.SaveChanges();

                return std.id; 
            }
            
                    
        }




        private static void obtenerPedidoDetalle(Pedido pedido, bool clienteNuevo)
        {
            List<PedidoDetalle> pedidos = new List<PedidoDetalle>();
            IFirebaseConfig confi = new FirebaseConfig()
            {
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"
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
                    PedidoId = pedido.Id,
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

                AgregarPedidoDetalleSigaAdmin(detalle, pedido.Id);

            }
        }
        private static void AgregarPedidoDetalleSigaAdmin(PedidoDetalle pedido, int pedidoid)
        {
            SigaAdminEntities context2 = new SigaAdminEntities();



            var t = new InvoiceLine
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
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"

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
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"
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
                    ID = get.Value.ID,
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
                    telefono2 = get.Value.telefono2, 
                    idFire = get.Key
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

            var t = new Customer //Make sure you have a table called test in DB
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
                IsInAx = false, 
                idFire = cliente.idFire

            };
            context.Customers.Add(t);
            context.SaveChanges();

            ActualizarCliente(cliente);
        }
        private static void ActualizarCliente(Clientes clientes)
        {
            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"

            };
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }


            Clientes toUpdate = new Clientes()
            {
                ID = clientes.ID, 
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


            var setter = client.Update("Clientes/" + clientes.idFire, toUpdate);
            Console.WriteLine("sincronizado");
        }


        /// <summary>
        /// Obtiene los clientes que fueron modificados, y que se debe de actualizar en firebase, para que esten correcto en la data del vendedor
        /// </summary>
        private static void  ObtenerClientesModificados(IFirebaseConfig confi)
        {
            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"

            };
            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }


           

            List<Customer> clientes; 

            using(var context = new SigaAdminEntities())
            {
                clientes = context.Customers.Where(x => x.subir != 0).ToList(); 

            }
            if(clientes.Count ==0)
            {
                return; 

            }
            foreach(Customer cliente in clientes)
            {
                Clientes toUpdate = new Clientes()
                {
                    
                    codigo = cliente.CustomerCode,
                    codigoVendedor = cliente.VendorCode,
                    direccion = cliente.CustomerDir,
                    activo = 1,
                    nombre = cliente.CustomerName,
                    comentario = cliente.Comment1,
                    
                    telefono1 = cliente.Phone1,
                    telefono2 = cliente.Phone2,
                    sincronizado = 0
                };


                client.Set("Clientes/" + toUpdate.idFire, toUpdate);
                marcarSincronizadoCliente(toUpdate.codigo);
            }
            
        }

        private static void marcarSincronizadoCliente(string clienteID)
        {
            using (var context = new SigaAdminEntities())
            {
                var resultado = context.Customers.SingleOrDefault(b => b.CustomerCode == clienteID);
                if (resultado != null)
                {
                    resultado.subir = 0;
                    context.SaveChanges();
                }
            }
        }

        //Facturas 
        private static void SincronizarFacturas(IFirebaseConfig confi)
        {
            SigaAdminEntities context2 = new SigaAdminEntities();
            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
            if (client == null)
            {
                return; 
            }

            var facturas = from a in context2.Facturas
                           where a.Sincronizado == 1
                           select a;

            foreach (Factura factura in facturas)
            {
                client.Set("Factura/" + factura.FacturaId, factura);
                
                updateFacturas(factura.FacturaId);

            }
            
            SincronizarFacturaDetalles(confi);
        }
        private static void updateFacturas(string facturaid)
        {
            using (var context = new SigaAdminEntities())
            {
                var resultado = context.Facturas.SingleOrDefault(b => b.FacturaId == facturaid);
                if (resultado != null)
                {
                    resultado.Sincronizado = 0;
                    context.SaveChanges();
                }
            }

        }

        private static void SincronizarFacturaDetalles(IFirebaseConfig confi)
        {
            SigaAdminEntities context2 = new SigaAdminEntities();
            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
            if (client == null)
            {
                return;
            }

            var detalles = from a in context2.FacturaDetalles
                           where a.Sincronizado == 1
                           select a;

            foreach (FacturaDetalle detalle in detalles)
            {
                client.Set("FacturaDetalle/" + detalle.ID + '-' + detalle.FacturaId, detalle);
                updateFacturaDetalles(detalle.ID);

            }
            
        }
        private static void updateFacturaDetalles(int id)
        {
            using (var context = new SigaAdminEntities())
            {
                var resultado = context.FacturaDetalles.SingleOrDefault(b => b.ID == id);
                if (resultado != null)
                {
                    resultado.Sincronizado = 0;
                    context.SaveChanges();
                }
            }

        }

        //Pagos 
        private static void obtenerPago(IFirebaseConfig confi)
        {
            //Lista de Clientes 
            List<Pago> pago = new List<Pago>();


            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }

            var pagoList = new List<Pago>();
            var respuesta = client.Get(@"Pago/");

            Dictionary<string, Pago> obtenerPagos = respuesta.ResultAs<Dictionary<string, Pago>>();

            foreach (var get in obtenerPagos)
            {
                pagoList.Add(new Pago
                {

                    ID = get.Key,
                    Banco = get.Value.Banco,

                    ClienteId = get.Value.ClienteId,
                    Compagni = get.Value.Compagni,
                    FechaDeCheque = get.Value.FechaDeCheque,
                    FechaPago = get.Value.FechaPago,
                    IsDelete = get.Value.IsDelete,
                    MetodoDePago = get.Value.MetodoDePago,
                    MontoPagado = get.Value.MontoPagado,
                    NumeroDeCheque = get.Value.NumeroDeCheque,
                    Pendiente = get.Value.Pendiente,
                    Sincronizado = get.Value.Sincronizado,
                    VendorId = get.Value.VendorId

                });



            };

            var pagosAsincronizar = pagoList.Where(x => x.Sincronizado == 1);

            foreach (var pagos in pagosAsincronizar)
            {

                agregarApagos(pagos);

                Console.WriteLine(pagos.ClienteId);
            }


        }
        private static void agregarApagos(Pago pago)
        {

            SigaAdminEntities context = new SigaAdminEntities();
            //SigaAppEntities context 

            var t = new PaymentOrder //Make sure you have a table called test in DB
            {
                VendorID = pago.VendorId,
                CheckDate = DateTime.Now,
                Datetime = DateTime.Now,
                Amount = pago.MontoPagado,
                Method = pago.MetodoDePago,
                BankName = pago.Banco,
                CheckNumber = pago.NumeroDeCheque,
                IsEnabled = true,
                Customer_Code = pago.ClienteId,
                IsOpen = true,
                Imported = false


            };
            context.PaymentOrders.Add(t);
            context.SaveChanges();

            

            //ActualizarCliente(pago);
        }

        
        private static void ActualizarPagoFire(Pago detalle)
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


            Pago toUpdate = new Pago()
            {
                Sincronizado = 0
            };


            var setter = client.Update("Pago/" + detalle.ID, toUpdate);
            Console.WriteLine("Pago Sincronizado" + detalle.ID);

        }

        private static void ObtenerPagoDetalle()
        {
            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "Qt94hB3EaVCBgvbQsTJ7c1UohQ9owGK05uyZALbF",
                BasePath = "https://siga-d5296-default-rtdb.firebaseio.com/"
            };

            //Lista de Clientes 
            List<PagoDetalle> pagoDetalle = new List<PagoDetalle>();


            IFirebaseClient client = new FireSharp.FirebaseClient(ifc);
            if (client == null)
            {
                return; 
                
            }

            var pagoDetalleLista = new List<PagoDetalle>();
            var respuesta = client.Get(@"PagoDetalle/");

            Dictionary<string, PagoDetalle> obtenerClientes = respuesta.ResultAs<Dictionary<string, PagoDetalle>>();

            foreach (var get in obtenerClientes)
            {
                pagoDetalle.Add(new PagoDetalle
                {
                    Compagnia = get.Value.Compagnia, 
                    facturaId = get.Value.facturaId, 
                    IsDelete = get.Value.IsDelete, 
                    montoAplicado = get.Value.montoAplicado, 
                    montoDeFacturaAlMomento = get.Value.montoDeFacturaAlMomento, 
                    pagoId = get.Value.pagoId, 
                    PagoIdFirebase = get.Value.PagoIdFirebase, 
                    sincronizado = get.Value.sincronizado 



                });


            };

            var pedidoASincronizar = pagoDetalle.Where(x => x.sincronizado == 1);

            foreach (var cliente in pedidoASincronizar)
            {

                //AgregarClienteSigaAdmin(cliente);

                //Console.WriteLine(cliente.nombre);
            }

            

        }


    }


}




     
 


