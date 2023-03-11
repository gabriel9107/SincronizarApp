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

namespace SigaApi
{
    class Program
    {
        SigaEntities context = new SigaEntities();
        SigaAdminEntities context2 = new SigaAdminEntities();

        static void Main(string[] args)
        {


            IFirebaseConfig ifc = new FirebaseConfig()
            {
                AuthSecret = "ryja3YG6bf0hAcJNXVpvUDdY66j0LiBFtfvRKMKK",
                BasePath = "https://sigaapp-127c4-default-rtdb.firebaseio.com/"
            };

            IFirebaseClient client;
            //obtenerClientes(ifc);
            obtenerPedido(ifc);
            //obtenerPedidoDetalle(ifc);
        }
       


        private static void obtenerClientes(IFirebaseConfig confi)
        {
            //Lista de Clientes 
            List<Cliente> clientes = new List<Cliente>();


            IFirebaseClient client = new FireSharp.FirebaseClient(confi);
            if (client != null)
            {
                Console.WriteLine("connection esta estabilizada");
            }

            var clientesList = new List<Clientes>();
            var respuesta = client.Get(@"Clientes/");

            Dictionary<string, Clientes> obtenerClientes = respuesta.ResultAs<Dictionary<string, Clientes>>();

            foreach (var get in obtenerClientes)
            {
                //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
                clientesList.Add(get.Value);

            };

            var ClienteASincronizar = clientesList.Where(x => x.sincronizado == "1");

            foreach (var cliente in ClienteASincronizar)
            {

                AgregarClienteSigaAdmin(cliente);

                Console.WriteLine(cliente.nombre);
            }





            Console.ReadLine();
            Console.Read();
        }
        private static void obtenerPedido(IFirebaseConfig confi)
        {
            List<Pedido2> pedidos = new List<Pedido2>();


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



                //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
                pedidoLista.Add(get.Value);



            };

            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

            foreach (var pedido in pedidosAsincronizar)
            {

                AgregarPedidosSigaAdmin(pedido);

                Console.WriteLine(pedido.id);
            }





            Console.ReadLine();
            Console.Read();
        }



        private static void obtenerPedidoDetalle(Invoice pedido)
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



                //Console.WriteLine(get.Value.nombre);value 1 to type 'SigaApi.Clientes'. Path 'ID', line 1, position 7.'
                pedidoLista.Add(get.Value);



            };

            var pedidosAsincronizar = pedidoLista.Where(x => x.Sincronizado == 1);

            foreach (var detalle in pedidosAsincronizar)
            {

                AgregarPedidoDetalleSigaAdmin(detalle, pedido.ID);
                 
            }





            Console.ReadLine();
            Console.Read();
        }
        private static void AgregarPedidoDetalleSigaAdmin(PedidoDetalle pedido, int pedidoid)
        {
            SigaAdminEntities context2 = new SigaAdminEntities();


            //var idProductoId = context2.
            //       .Where(s => s.CustomerCode == pedido.ClienteId)
            //        .FirstOrDefault<Customer>();


            var t = new InvoiceLine //Make sure you have a table called test in DB
            {
                Qty = pedido.Cantidad, 
                InvoiceID = pedidoid, 
                Price = pedido.Precio, 
                ProductCode = pedido.Codigo, 
             
                 
                 
                IsNew = true,
                IsInAx = false



            };
            context2.InvoiceLines.Add(t);
            context2.SaveChanges();

            ActualizarPedidoDetalleFire(pedido);
        }
        private static void AgregarPedidosSigaAdmin(Pedido pedido)
        {
            SigaAdminEntities context2 = new SigaAdminEntities();


            var id = context2.Customers
                   .Where(s => s.CustomerCode == pedido.ClienteId)
                    .FirstOrDefault<Customer>();


            var t = new Invoice //Make sure you have a table called test in DB
            {
                Date = (DateTime)pedido.FechaOrden,
                CustomerID = id.ID,
                //CustomerID = pedido.ClienteId,
                Totals = pedido.totalAPagar,
                VAT = pedido.Impuestos,
                VendorCode = pedido.CreadoPor,
                Status = 1,
                IsNew = true,
                IsInAx = false



            };
            context2.Invoices.Add(t);
            context2.SaveChanges();


            obtenerPedidoDetalle(t);
            //Console.WriteLine("Get Id " + t.ID);
            //ActualizarPedidoFire(pedido);
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


            Pedido toUpdate = new Pedido()
            {
                Sincronizado = 0
            };


            var setter = client.Update("PedidoDetalle/" + detalle.PedidoId, toUpdate);
            Console.WriteLine("Pedido Sincronizado" + detalle.PedidoId);

        }
        private static void ActualizarPedidoFire(Pedido pedido)
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


            Pedido toUpdate = new Pedido()
            {
                Sincronizado = 0
            };


            var setter = client.Update("Pedidos/" + pedido.id, toUpdate);
            Console.WriteLine("Pedido Sincronizado" + pedido.id);
        }

        private static void AgregarClienteSigaAdmin(Clientes cliente)
        {

            SigaAdminEntities context2 = new SigaAdminEntities();
            var t = new Customer //Make sure you have a table called test in DB
            {
                CustomerCode = cliente.codigo,
                CustomerName = cliente.nombre,
                CustomerDir = cliente.direccion,
                Phone1 = cliente.Teleftelefono1ono1,
                //ShippingMth = cliente.
                VendorCode = cliente.codigoVendedor,
                Comment1 = cliente.comentario,
                IsNew = true,
                IsInAx = false
            };
            context2.Customers.Add(t);
            context2.SaveChanges();

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
                Teleftelefono1ono1 = clientes.Teleftelefono1ono1,
                sincronizado = "0"
            };


            var setter = client.Update("Clientes/" + clientes.ID, toUpdate);
            Console.WriteLine("sincronizado");
        }

       
    }
}
