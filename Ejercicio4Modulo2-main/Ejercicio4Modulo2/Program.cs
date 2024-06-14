using Ejercicio4Modulo2.Domain.Entity;
using Ejercicio4Modulo2.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Ejercicio4Modulo2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}\\data.txt";
            var options = new DbContextOptionsBuilder<Ejercicio4Modulo2Context>();
            options.UseSqlServer("Data Source=localhost;Initial Catalog=Ejercicio4Modulo2;Integrated Security=True;Trust Server Certificate=True");
            var context = new Ejercicio4Modulo2Context(options.Options);
           
            var ventasVl = context.VentasMensuales.ToList();            
            bool validado;

            var fils = File.ReadAllLines(path);
            foreach (var fil in fils) {

                var fecha = fil.Substring(0, 10);
                var codigo = fil.Substring(10, 3);
                var venta = fil.Substring(13, 11);
                var ventmMay = fil.Substring(24, 1);
           
              
                bool vgemp;

                if (ventmMay == "S")
                {
                    vgemp = true;
                }
                else
                {
                    vgemp = false;
                }
               
                
                validado = validacion(codigo, ventmMay, context, fecha);

                if (!validado)
                {

                    var err = error(codigo, ventmMay, context, fecha);
                    
                    var rechazos = new Rechazo() {
                        Error = err,
                        RegistroOriginal=fil
                    };


                    var rechazosRepetidos = context.Rechazos.Where(x => x.RegistroOriginal == fil).ToList();

                    if (!rechazosRepetidos.Any())
                    {
                        context.Add(rechazos);
                        context.SaveChanges();
                    }
                }
                else { 
                 var ventas = new VentasMensuale()
                    {

                        Fecha = DateTime.Parse(fecha),
                        CodVendedor = codigo,
                        Venta = decimal.Parse(venta),
                        VentaEmpresaGrande = vgemp
                    };
                    var lista = new List<VentasMensuale>();
                    lista.Add(ventas);
                    var ventasRepetidas = context.VentasMensuales.Where(x => x.Fecha == ventas.Fecha && x.CodVendedor == ventas.CodVendedor 
                    && x.Venta == ventas.Venta && x.VentaEmpresaGrande == ventas.VentaEmpresaGrande).ToList();

                    if (!ventasRepetidas.Any())
                    {
                        context.VentasMensuales.Add(ventas);
                        context.SaveChanges();
                    }

                     
                }
            }

    //Listar todos los vendedores que hayan superado los 100.000 en el mes.Ejemplo: "El vendedor 001 vendio 250.000"

            var result1 = context.VentasMensuales.Where( VentasMensuale => VentasMensuale.Venta > 100000).ToList();

            foreach (var VentasMensuale in result1)
            {
                var codigoMayores = VentasMensuale.CodVendedor;
                var ventaMayores = VentasMensuale.Venta;
                Console.WriteLine("El vendedor " + codigoMayores + " vendio " + ventaMayores);
            }
    //Listar todos los vendedores que NO hayan superado los 100.000 en el mes.Ejemplo: "El vendedor 001 vendio 90.000"
            var result2 = context.VentasMensuales.Where(VentasMensuale => VentasMensuale.Venta < 100000).ToList();

            foreach (var VentasMensuale in result2)
            {
                var codigoMenores = VentasMensuale.CodVendedor;
                var ventaMenores = VentasMensuale.Venta;
                Console.WriteLine("El vendedor " +codigoMenores+ " vendio " +ventaMenores);
            }
    //Listar todos los vendedores que haya  vendido al menos una vez a una empresa grande.Solo listar los codigos de vendedor

            var result3 = context.VentasMensuales.Where(VentasMensuale => VentasMensuale.VentaEmpresaGrande == true).Select( p =>  p.CodVendedor).ToList();

            foreach (var VentasMensuale in result2)
            {
                var CodVendGrandes = VentasMensuale.CodVendedor;
                Console.WriteLine("El vendedor " + CodVendGrandes);
            }
            //lista de rechazado

            var resul =context.Rechazos.ToList();

            foreach (var lisRechazados in resul)
            {

                var id = lisRechazados.Id;
                var error = lisRechazados.Error;
                var registroOriginal = lisRechazados.RegistroOriginal;

                Console.WriteLine(id+" "+error+" "+registroOriginal);

            }

        }

        public static bool  validacion(string campos,string ns,Ejercicio4Modulo2Context context, string Fecha) {
            
          bool validado=true; 

          if( campos == "   ") {
 
                validado=false ;
            }
          if( ns =="X") {

                validado = false; 
            }

            var fechaParam = context.Parametria.ToList();

            foreach (var fecha in fechaParam)
            {

                var fechaparametria = fecha.Value;

                if (fechaparametria != Fecha)
                {
                    validado = false;
                }


            }
            return validado;
        }
        public static string error(string campos, string ns, Ejercicio4Modulo2Context context, string Fecha)
        {
            var error = "";
            bool validado = true;
            if (campos == "   ")
            {
               error = "No tiene código";               
            }
            if (ns == "X")
            {
                error = "El Tamaño de la venta es distinto N y S";
               
            }

            var fechaParam = context.Parametria.ToList();


            foreach (var fecha in fechaParam)
            {

                var fechaparametria = fecha.Value;

                if (fechaparametria != Fecha)
                {
                    error = "Fecha incorrecta";
                    
                    validado = false;

                }


            }
            return error;
        }


    }
}