using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace generacionArchivo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string connectionString = "datasource=localhost;port=3306;username=root;password=root;database=reportes;";
            string query = "call Consulta_Totales";
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, databaseConnection);
            commandDatabase.CommandTimeout = 60;
            MySqlDataReader reader;

            try
            {
                databaseConnection.Open();
                reader = commandDatabase.ExecuteReader();

                Document doc = new Document();
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = @"C:";
                saveFileDialog1.Title = "Guardar Reporte";
                saveFileDialog1.DefaultExt = "pdf";
                saveFileDialog1.Filter = "pdf Files (*.pdf)|*.pdf| All Files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;
                string filename = "";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    filename = saveFileDialog1.FileName;
                }
                if (filename.Trim() != "")
                {
                    FileStream file = new FileStream(filename,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                    PdfWriter.GetInstance(doc, file);
                    doc.Open();
                    string envio = "Fecha:" + DateTime.Now.ToString();
                    
                    Chunk chunk = new Chunk("Reporte de General "+ envio, FontFactory.GetFont("ARIAL", 20, iTextSharp.text.Font.BOLD));
                    doc.Add(new Paragraph(chunk));
                    doc.Add(new Paragraph("------------------------------------------------------------------------------------------"));

                    if (reader.HasRows)
                    {
                        bool CONTRATACION = true;
                        bool TV = true;
                        bool INTERNET = true;
                        bool TV_INTERNET = true;
                        bool desgloce = true;

                        List<datos> InformacionData = new List<datos>();
                        List<datos> InformacionDataDesgloce = new List<datos>();
                        while (reader.Read())
                        {
                            datos informacion = new datos();

                            informacion.id = reader.GetString(0);
                            informacion.nombre_completo = reader.GetString(1)+" "+ reader.GetString(2);
                            informacion.total = reader.GetString(3);
                            informacion.totalAdic = reader.GetString(4);
                            informacion.descripcion = reader.GetString(5);
                            informacion.tipoServicio = reader.GetString(6);
                            informacion.tipoPaquete = reader.GetString(7);
                            informacion.servicio = reader.GetString(8);

                            if (informacion.tipoServicio == "")
                                InformacionData.Add(informacion);
                            else
                                InformacionDataDesgloce.Add(informacion);
                        }

                        foreach (var data in InformacionData)
                        {
                            if (data.descripcion.Contains("CONTRATACION(ADICIONALES)") && CONTRATACION == true)
                            {
                                doc.Add(new Paragraph("CONTRATACION(ADICIONALES) $" + data.totalAdic, FontFactory.GetFont("ARIAL", 15, iTextSharp.text.Font.BOLD)));
                                CONTRATACION = false;
                            }
                            if (data.descripcion.Contains("SERVICIOS TV") && TV == true)
                            {
                                doc.Add(new Paragraph("SERVICIOS TV $" + data.totalAdic, FontFactory.GetFont("ARIAL", 15, iTextSharp.text.Font.BOLD)));
                                TV = false;
                            }
                            if (data.descripcion.Contains("SERVICIOS INTERNET") && INTERNET == true)
                            {
                                doc.Add(new Paragraph("SERVICIOS INTERNET $" + data.totalAdic, FontFactory.GetFont("ARIAL", 15, iTextSharp.text.Font.BOLD)));
                                INTERNET = false;
                            }
                            if (data.descripcion.Contains("SERVICIOS TV_INTERNET") && TV_INTERNET == true)
                            {
                                doc.Add(new Paragraph("SERVICIOS TV_INTERNET $" + data.totalAdic, FontFactory.GetFont("ARIAL", 15, iTextSharp.text.Font.BOLD)));
                                TV_INTERNET = false;
                            }
                            if (data.tipoServicio == "")
                            {
                                doc.Add(new Paragraph(" --- " + data.nombre_completo + "   $" + data.total));
                                foreach (var Desgloce in InformacionDataDesgloce)
                                {
                                    if (Desgloce.id == data.id && Desgloce.servicio == data.servicio && desgloce==true) {
                                        doc.Add(new Paragraph(" - DESGLOSE - ", FontFactory.GetFont("ARIAL", 10, iTextSharp.text.Font.BOLD)));
                                        desgloce = false;
                                    }
                                    if (Desgloce.id == data.id && Desgloce.servicio == data.servicio)
                                    {
                                        doc.Add(new Paragraph(" --- " + Desgloce.descripcion + "   $" + Desgloce.total));
                                    }

                                }

                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No se encontraron datos.");
                    }
                    
                    //GenerarDocumento(doc);
                    doc.AddCreationDate();
                    doc.Add(new Paragraph("______________________________________________", FontFactory.GetFont("ARIAL", 20, iTextSharp.text.Font.BOLD)));
                    doc.Close();
                    Process.Start(filename);//Esta parte se puede omitir, si solo se desea guardar el archivo, y que este no se ejecute al instante
                }

                // Cerrar la conexión
                databaseConnection.Close();
            }
            catch (Exception ex)
            {
                // Mostrar cualquier excepción
                MessageBox.Show(ex.Message);
            }
        }
    }
}
