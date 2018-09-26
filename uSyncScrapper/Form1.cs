﻿using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using uSyncScrapper.Models;

namespace uSyncScrapper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBrowseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void buttonScrap_Click(object sender, EventArgs e)
        {
            ParseUSyncfilesToHtml(textBox1.Text);
        }

        private void ParseUSyncfilesToHtml(string folder)
        {
            var docTypes = ParseXMLFiles(folder);
            GenerateHtml(docTypes);
        }

        private IEnumerable<DocumentType> ParseXMLFiles(string folder)
        {
            var docTypes = new List<DocumentType>();
            string[] files = Directory.GetFiles(folder, "*.config", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var docType = new DocumentType();
                docTypes.Add(docType);
                XDocument doc = XDocument.Load(file);

                var name = doc
                    .Root
                    .Element("Info")
                    .Element("Name")
                    .Value;
                docType.Name = name;

                var description = doc
                    .Root
                    .Element("Info")
                    .Element("Description")
                    .Value;
                docType.Description = description;

                var properties = doc
                    .Root
                    .Element("GenericProperties")
                    .Elements("GenericProperty")
                    .Select(i => new DocumentTypeProperty { Name = i.Element("Name").Value, Text = i.Element("Description").Value });
                docType.Properties = properties;
                
            }
            return docTypes;
        }

        private static void GenerateHtml(IEnumerable<DocumentType> docTypes)
        {
            string documentTypeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "DocumentType.cshtml");
            string finalDocumentFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "FinalDocument.cshtml");

            var templateService = new TemplateService();
            templateService.AddNamespace("uSyncScrapper.Models");
            var body = new StringBuilder();

            foreach (var docType in docTypes)
            {
                body.Append(templateService.Parse(File.ReadAllText(documentTypeFilePath), docType, null, "DocumentType"));
            }

            var finalDocType = new FinalDocument { Body = body.ToString() };
            var finalDocument = templateService.Parse(File.ReadAllText(finalDocumentFilePath), finalDocType, null, "FinalDocument");
        }
    }
}