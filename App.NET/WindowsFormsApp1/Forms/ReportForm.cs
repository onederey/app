﻿using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using WindowsFormsApp1.Classes;

namespace WindowsFormsApp1.Forms
{
	public partial class ReportFor_m : Form
	{
		private readonly App app = new App();
		private string currentReport;
		private int paramsCount = 0;

		public ReportFor_m(string report)
		{
			InitializeComponent();
			currentReport = report;
			this.Text = currentReport;
			paramsCount = 0;
			HideParamsBoxes();
			UpdateReportData();
		}

		private void HideParamsBoxes()
		{
			textBoxParam1.Visible = false;
			textBoxParam2.Visible = false;
			textBoxParam3.Visible = false;

			paramLabel1.Visible = false;
			paramLabel2.Visible = false;
			paramLabel3.Visible = false;
		}

		private void UpdateReportData()
		{
			var paramSP = GetReportParams().Tables[0].Rows;
			switch (paramSP.Count)
			{
				case 0:
					break;
				case 1:
					textBoxParam1.Visible = true;
					paramLabel1.Visible = true;
					paramLabel1.Text = paramSP[0].ItemArray[1].ToString() + " (" + paramSP[0].ItemArray[2].ToString() + "):";
					paramsCount++;
					break;
				case 2:
					textBoxParam1.Visible = true;
					paramLabel1.Visible = true;
					paramLabel1.Text = paramSP[0].ItemArray[1].ToString() + " (" + paramSP[0].ItemArray[2].ToString() + "):";
					paramsCount++;

					textBoxParam2.Visible = true;
					paramLabel2.Visible = true;
					paramLabel2.Text = paramSP[1].ItemArray[1].ToString() + " (" + paramSP[1].ItemArray[2].ToString() + "):";
					paramsCount++;
					break;
				case 3:
					textBoxParam1.Visible = true;
					paramLabel1.Visible = true;
					paramLabel1.Text = paramSP[0].ItemArray[1].ToString() + " (" + paramSP[0].ItemArray[2].ToString() + "):";
					paramsCount++;

					textBoxParam2.Visible = true;
					paramLabel2.Visible = true;
					paramLabel2.Text = paramSP[1].ItemArray[1].ToString() + " (" + paramSP[1].ItemArray[2].ToString() + "):";
					paramsCount++;

					textBoxParam3.Visible = true;
					paramLabel3.Visible = true;
					paramLabel3.Text = paramSP[2].ItemArray[1].ToString() + " (" + paramSP[2].ItemArray[2].ToString() + "):";
					paramsCount++;
					break;
				default:
					break;
			}
		}

		private string GetReportSP() => SqlHelper.GetReportSP(currentReport);

		private DataSet GetReportParams() => SqlHelper.GetSPParams(GetReportSP());

		/// <summary>
		/// CREATE REPORT
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				SqlParameter param1 = new SqlParameter();
				SqlParameter param2 = new SqlParameter();
				SqlParameter param3 = new SqlParameter();
				var ds = new DataSet();

				switch (paramsCount)
				{
					case 0:
						break;
					case 1:
						param1 = CreateSqlParameter(paramLabel1.Text, string.IsNullOrEmpty(textBoxParam1.Text) ? null : textBoxParam1.Text);
						ds = SqlHelper.ExecSpWithParams(GetReportSP(), param1);
						break;
					case 2:
						param1 = CreateSqlParameter(paramLabel1.Text, string.IsNullOrEmpty(textBoxParam1.Text) ? null : textBoxParam1.Text);
						param2 = CreateSqlParameter(paramLabel2.Text, string.IsNullOrEmpty(textBoxParam2.Text) ? null : textBoxParam2.Text);
						ds = SqlHelper.ExecSpWithParams(GetReportSP(), param1, param2);
						break;
					case 3:
						param1 = CreateSqlParameter(paramLabel1.Text, string.IsNullOrEmpty(textBoxParam1.Text) ? null : textBoxParam1.Text);
						param2 = CreateSqlParameter(paramLabel2.Text, string.IsNullOrEmpty(textBoxParam2.Text) ? null : textBoxParam2.Text);
						param3 = CreateSqlParameter(paramLabel3.Text, string.IsNullOrEmpty(textBoxParam3.Text) ? null : textBoxParam3.Text);
						ds = SqlHelper.ExecSpWithParams(GetReportSP(), param1, param2, param3);
						break;
					default:
						break;
				}

				if (ds != default)
				{
					dataGridView1.DataSource = null;
					dataGridView1.AutoGenerateColumns = true;
					dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
					dataGridView1.DataSource = ds;
					dataGridView1.DataMember = ds.Tables?[0].TableName;
					dataGridView1.ReadOnly = true;
				}
			}
			catch(Exception ex)
			{
				app.LogError(ex);
			}
		}

		private SqlParameter CreateSqlParameter(string param, object value)
		{
			SqlParameter parameter = new SqlParameter();
			var splitParam = param.Split(' ');
			parameter.ParameterName = splitParam[0];
			parameter.Value = value;

			if (splitParam[1].Contains("DATE"))
				parameter.SqlDbType = SqlDbType.Date;
			else if (splitParam[1].Contains("SMALLINT"))
				parameter.SqlDbType = SqlDbType.SmallInt;
			else if (splitParam[1].Contains("INT"))
				parameter.SqlDbType = SqlDbType.Int;
			else if (splitParam[1].Contains("NVARCHAR"))
				parameter.SqlDbType = SqlDbType.NVarChar;

			return parameter;
		}

		/// <summary>
		/// EXPORT TO PDF
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void exportButton_Click(object sender, EventArgs e)
		{
			if (dataGridView1.Rows.Count > 0)
			{
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.Filter = "PDF (*.pdf)|*.pdf";
				sfd.FileName = $"{currentReport} - {DateTime.Now.ToShortDateString()}.pdf";
				bool fileError = false;
				if (sfd.ShowDialog() == DialogResult.OK)
				{
					if (File.Exists(sfd.FileName))
					{
						try
						{
							File.Delete(sfd.FileName);
						}
						catch (IOException ex)
						{
							fileError = true;
							app.LogError(ex);
						}
					}
					if (!fileError)
					{
						try
						{
							iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(new FileStream(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../Resources/logoImage.bmp"), FileMode.Open));
							logo.SetAbsolutePosition((PageSize.A4.Width - logo.ScaledWidth), (PageSize.A4.Height - logo.ScaledHeight));

							PdfPTable pdfTable = new PdfPTable(dataGridView1.Columns.Count);
							pdfTable.DefaultCell.Padding = 3;
							pdfTable.WidthPercentage = 100;
							pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

							foreach (DataGridViewColumn column in dataGridView1.Columns)
							{
								PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
								pdfTable.AddCell(cell);
							}

							foreach (DataGridViewRow row in dataGridView1.Rows)
							{
								foreach (DataGridViewCell cell in row.Cells)
								{
									pdfTable.AddCell(cell.Value.ToString());
								}
							}

							using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
							{
								Document pdfDoc = new Document(PageSize.A4, 10f, 20f, 20f, 10f);
								PdfWriter.GetInstance(pdfDoc, stream);
								pdfDoc.Open();
								pdfDoc.Add(new Paragraph($"{currentReport} - report, generated {DateTime.Now}.\n\nAuto-reporting system.\n\n"));
								pdfDoc.Add(pdfTable);
								pdfDoc.Add(logo);
								pdfDoc.Close();
								stream.Close();
							}

							app.LogSuccess("Data Exported Successfully!");
						}
						catch (Exception ex)
						{
							app.LogError(ex);
						}
					}
				}
			}
			else
			{
				app.LogInfo("No records to export!");
			}
		}
	}
}
