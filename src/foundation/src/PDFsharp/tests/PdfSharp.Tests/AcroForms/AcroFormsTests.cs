using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.StandardFonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Annotations.enums;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace PdfSharp.Tests.AcroForms
{
    public class AcroFormsTests
    {
        private readonly ITestOutputHelper output;

        public AcroFormsTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;

            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(Path.Combine(baseDir!, "..", "..", ".."));
        }

        [Fact]
        public void CanImportForm()
        {
            IOUtility.EnsureAssets("pdfsharp-6.x\\pdfs\\DocumentWithAcroForm.pdf");
            var pdfPath = IOUtility.GetAssetsPath("pdfsharp-6.x\\pdfs\\DocumentWithAcroForm.pdf")!;
            using var fs = File.OpenRead(pdfPath);
            var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);

            PdfPage? lastPage = null;
            // import into new document
            var copiedDocument = new PdfDocument();
            foreach (var page in inputDocument.Pages)
            {
                copiedDocument.AddPage(page);
                lastPage = page;
            }
            // import AcroForm
            copiedDocument.ImportAcroForm(inputDocument.AcroForm!);

            copiedDocument.AcroForm.Should().NotBeNull();

            var fieldsInInputDocument = GetAllFields(inputDocument);
            var fieldsInCopiedDocument = GetAllFields(copiedDocument);
            fieldsInCopiedDocument.Count.Should().Be(fieldsInInputDocument.Count);

            // fill all fields
            FillFields(fieldsInCopiedDocument);

            var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/FilledForm.pdf");
            using var fsOut = File.Create(outFileName);
            copiedDocument.Save(fsOut);
            fsOut.Close();

            VerifyFieldsFilledWithDefaultValues(outFileName);
        }

        [Fact]
        public void CanImportMultipleForms()
        {
            IOUtility.EnsureAssets("pdfsharp-6.x\\pdfs\\DocumentWithAcroForm.pdf");
            IOUtility.EnsureAssets("pdfsharp-6.x\\pdfs\\DemoFormWithCombs.pdf");
            var pdfPath = IOUtility.GetAssetsPath("pdfsharp-6.x\\pdfs")!;

            GlobalFontSettings.ResetAll();
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var files = new[] { "DocumentWithAcroForm.pdf", "DemoFormWithCombs.pdf" };
            var copiedDocument = new PdfDocument();
            var importedFields = new List<PdfAcroField>();
            foreach (var file in files)
            {
                using var fs = File.OpenRead(Path.Combine(pdfPath, file));
                var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);
                foreach (var page in inputDocument.Pages)
                    copiedDocument.AddPage(page);
                copiedDocument.ImportAcroForm(inputDocument.AcroForm!);
                importedFields.AddRange(GetAllFields(inputDocument));
            }
            var fieldsInCopiedDocument = GetAllFields(copiedDocument);
            fieldsInCopiedDocument.Count.Should().Be(importedFields.Count);

            FillFields(fieldsInCopiedDocument);

            var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/FilledForm_multiple.pdf");
            using var fsOut = File.Create(outFileName);
            copiedDocument.Save(fsOut);
            fsOut.Close();

            VerifyFieldsFilledWithDefaultValues(outFileName);
        }

        [Fact]
        public void CanImportSameFormMultipleTimes()
        {
            IOUtility.EnsureAssets("pdfsharp-6.x\\pdfs\\DocumentWithAcroForm.pdf");
            var pdfPath = IOUtility.GetAssetsPath("pdfsharp-6.x\\pdfs")!;

            GlobalFontSettings.ResetAll();
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var files = new[] { "DocumentWithAcroForm.pdf", "DocumentWithAcroForm.pdf" };
            var copiedDocument = new PdfDocument();
            var importedFields = new List<PdfAcroField>();
            foreach (var file in files)
            {
                using var fs = File.OpenRead(Path.Combine(pdfPath, file));
                var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);
                foreach (var page in inputDocument.Pages)
                    copiedDocument.AddPage(page);
                copiedDocument.ImportAcroForm(inputDocument.AcroForm!, null, (remoteField, localField) =>
                {
                    output.WriteLine("Field import: {0} -> {1}", remoteField.FullyQualifiedName, localField.FullyQualifiedName);
                });
                importedFields.AddRange(GetAllFields(inputDocument));
            }
            var fieldsInCopiedDocument = GetAllFields(copiedDocument);
            fieldsInCopiedDocument.Count.Should().Be(importedFields.Count);
            // root field names should be distinct
            var rootNames = copiedDocument.AcroForm!.Fields.Names;
            rootNames.Distinct().Count().Should().Be(rootNames.Length);

            FillFields(fieldsInCopiedDocument);

            var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/FilledForm_same_multiple.pdf");
            using var fsOut = File.Create(outFileName);
            copiedDocument.Save(fsOut);
            fsOut.Close();

            VerifyFieldsFilledWithDefaultValues(outFileName);
        }

        [Fact]
        public void CanFilterFieldsDuringImport()
        {
            IOUtility.EnsureAssets("pdfsharp-6.x\\pdfs\\DocumentWithAcroForm.pdf");
            var pdfPath = IOUtility.GetAssetsPath("pdfsharp-6.x\\pdfs")!;

            GlobalFontSettings.ResetAll();
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            using var fs = File.OpenRead(Path.Combine(pdfPath, "DocumentWithAcroForm.pdf"));
            var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);

            PdfPage? lastPage = null;
            // import into new document
            var copiedDocument = new PdfDocument();
            foreach (var page in inputDocument.Pages)
            {
                copiedDocument.AddPage(page);
                lastPage = page;
            }
            // import AcroForm
            copiedDocument.ImportAcroForm(inputDocument.AcroForm!,
                remoteField =>
                {
                    // only import TextFields
                    return remoteField is PdfTextField;
                },
                (remoteField, localField) =>
                {
                    output.WriteLine("Field import: {0} -> {1}", remoteField.FullyQualifiedName, localField.FullyQualifiedName);
                }
            );

            copiedDocument.AcroForm.Should().NotBeNull();

            var fieldsInInputDocument = GetAllFields(inputDocument).Where(f => f is PdfTextField).ToList();
            var fieldsInCopiedDocument = GetAllFields(copiedDocument);
            fieldsInCopiedDocument.Count.Should().Be(fieldsInInputDocument.Count);

            // fill all fields
            FillFields(fieldsInCopiedDocument);

            var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/FilledForm_filtered.pdf");
            using var fsOut = File.Create(outFileName);
            copiedDocument.Save(fsOut);
            fsOut.Close();

            VerifyFieldsFilledWithDefaultValues(outFileName);
        }

        [Fact]
        public void ReturnsCorrectChildFields()
        {
            GlobalFontSettings.ResetAll();
            // we use one of the 14 standard-fonts here (Helvetica)
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var document = new PdfDocument();
            var page1 = document.AddPage();
            var acroForm = document.GetOrCreateAcroForm();
            var textFont = new XFont(StandardFontNames.Helvetica, 12, XFontStyleEx.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.EmbedCompleteFontFile));

            var personField = acroForm.AddGenericField(field =>
            {
                field.Name = "Person";
                field.Flags = PdfAcroFieldFlags.DoNotSpellCheckTextField;
            });
            var nameField = acroForm.AddGenericField(field =>
            {
                field.Name = "Name";
                field.Flags = PdfAcroFieldFlags.Required;
                field.DefaultValue = new PdfString("Please enter");
                personField.AddChild(field);
            });
            var addressField = acroForm.AddGenericField(field =>
            {
                field.Name = "Address";
                personField.AddChild(field);
            });
            var titleField = acroForm.AddTextField(field =>
            {
                field.Name = "Title";
                field.MaxLength = 100;
                field.Font = textFont;
                nameField.AddChild(field);
            });
            var firstNameField = acroForm.AddTextField(field =>
            {
                field.Name = "FirstName";
                field.MaxLength = 100;
                field.Font = textFont;
                nameField.AddChild(field);
            });
            var lastNameField = acroForm.AddTextField(field =>
            {
                field.Name = "LastName";
                field.MaxLength = 100;
                field.Font = textFont;
                nameField.AddChild(field);
            });
            var zipCodeField = acroForm.AddTextField(field =>
            {
                field.Name = "ZipCode";
                field.MaxLength = 10;
                field.Font = textFont;
                addressField.AddChild(field);
            });
            var cityField = acroForm.AddTextField(field =>
            {
                field.Name = "City";
                field.MaxLength = 100;
                field.Font = textFont;
                addressField.AddChild(field);
            });
            var streetField = acroForm.AddTextField(field =>
            {
                field.Name = "Street";
                field.MaxLength = 100;
                field.Font = textFont;
                addressField.AddChild(field);
            });
            var streetNumberField = acroForm.AddTextField(field =>
            {
                field.Name = "StreetNumber";
                field.MaxLength = 10;
                field.Font = textFont;
                addressField.AddChild(field);
            });

            var personFieldFromForm = acroForm.Fields["Person"]!;
            personFieldFromForm.Should().NotBeNull();

            var nameFieldFromForm = personFieldFromForm.Fields["Name"]!;
            nameFieldFromForm.Should().NotBeNull();
            nameFieldFromForm.FullyQualifiedName.Should().Be("Person.Name");
            var firstNameFromForm = nameFieldFromForm["FirstName"]!;
            firstNameFromForm.Should().NotBeNull();
            firstNameFromForm.FullyQualifiedName.Should().Be("Person.Name.FirstName");
            var lastNameFromForm = nameFieldFromForm["LastName"]!;
            lastNameFromForm.Should().NotBeNull();
            lastNameFromForm.FullyQualifiedName.Should().Be("Person.Name.LastName");

            var addressFieldFromForm = personFieldFromForm["Address"]!;
            addressFieldFromForm.Should().NotBeNull();
            addressFieldFromForm.FullyQualifiedName.Should().Be("Person.Address");
            var streetFieldFromForm = addressFieldFromForm["Street"]!;
            streetFieldFromForm.Should().NotBeNull();
            streetFieldFromForm.FullyQualifiedName.Should().Be("Person.Address.Street");
            var cityFieldFromForm = addressFieldFromForm["City"]!;
            cityFieldFromForm.Should().NotBeNull();
            cityFieldFromForm.FullyQualifiedName.Should().Be("Person.Address.City");
        }

        [Fact]
        public void PropertiesAreInheritedFromParent()
        {
            GlobalFontSettings.ResetAll();
            // we use one of the 14 standard-fonts here (Helvetica)
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var document = new PdfDocument();
            var page1 = document.AddPage();
            var acroForm = document.GetOrCreateAcroForm();
            var textFont = new XFont(StandardFontNames.Helvetica, 12, XFontStyleEx.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.EmbedCompleteFontFile));

            //double x = 40, y = 40;
            //var page1Renderer = XGraphics.FromPdfPage(page1);

            var personField = acroForm.AddGenericField(field =>
            {
                field.Name = "Person";
                field.Flags = PdfAcroFieldFlags.DoNotSpellCheckTextField;
            });
            var nameField = acroForm.AddGenericField(field =>
            {
                field.Name = "Name";
                field.Flags = PdfAcroFieldFlags.Required;
                field.DefaultValue = new PdfString("Please enter");
                personField.AddChild(field);
            });
            var addressField = acroForm.AddGenericField(field =>
            {
                field.Name = "Address";
                personField.AddChild(field);
            });
            var titleField = acroForm.AddTextField(field =>
            {
                field.Name = "Title";
                field.MaxLength = 100;
                field.Font = textFont;
                nameField.AddChild(field);
            });
            var firstNameField = acroForm.AddTextField(field =>
            {
                field.Name = "FirstName";
                field.MaxLength = 100;
                field.Font = textFont;
                nameField.AddChild(field);
            });
            var lastNameField = acroForm.AddTextField(field =>
            {
                field.Name = "LastName";
                field.MaxLength = 100;
                field.Font = textFont;
                nameField.AddChild(field);
            });
            var zipCodeField = acroForm.AddTextField(field =>
            {
                field.Name = "ZipCode";
                field.MaxLength = 10;
                field.Font = textFont;
                addressField.AddChild(field);
            });
            var cityField = acroForm.AddTextField(field =>
            {
                field.Name = "City";
                field.MaxLength = 100;
                field.Font = textFont;
                addressField.AddChild(field);
            });
            var streetField = acroForm.AddTextField(field =>
            {
                field.Name = "Street";
                field.MaxLength = 100;
                field.Font = textFont;
                addressField.AddChild(field);
            });
            var streetNumberField = acroForm.AddTextField(field =>
            {
                field.Name = "StreetNumber";
                field.MaxLength = 10;
                field.Font = textFont;
                addressField.AddChild(field);
            });

            personField.Fields.Names.Length.Should().Be(2);
            personField.Fields.Names.Should().Equal(["Name", "Address"]);
            nameField.Fields.Names.Length.Should().Be(3);
            nameField.Fields.Names.Should().Equal(["Title", "FirstName", "LastName"]);
            addressField.Fields.Names.Length.Should().Be(4);
            addressField.Fields.Names.Should().Equal(["ZipCode", "City", "Street", "StreetNumber"]);

            var allFields = acroForm.GetAllFields();
            allFields.Count().Should().Be(10);
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Name");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Address");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Name.Title");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Name.FirstName");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Name.LastName");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Address.ZipCode");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Address.City");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Address.Street");
            allFields.Should().Contain(f => f.FullyQualifiedName == "Person.Address.StreetNumber");

            zipCodeField.Flags.Should().Be(PdfAcroFieldFlags.DoNotSpellCheckTextField);
            cityField.Flags.Should().Be(PdfAcroFieldFlags.DoNotSpellCheckTextField);
            streetField.Flags.Should().Be(PdfAcroFieldFlags.DoNotSpellCheckTextField);
            streetNumberField.Flags.Should().Be(PdfAcroFieldFlags.DoNotSpellCheckTextField);

            titleField.Flags.Should().Be(PdfAcroFieldFlags.Required);
            firstNameField.Flags.Should().Be(PdfAcroFieldFlags.Required);
            lastNameField.Flags.Should().Be(PdfAcroFieldFlags.Required);
            titleField.DefaultValue.Should().BeOfType<PdfString>();
            ((PdfString)titleField.DefaultValue!).Value.Should().Be("Please enter");
            firstNameField.DefaultValue.Should().BeOfType<PdfString>();
            ((PdfString)firstNameField.DefaultValue!).Value.Should().Be("Please enter");
            lastNameField.DefaultValue.Should().BeOfType<PdfString>();
            ((PdfString)lastNameField.DefaultValue!).Value.Should().Be("Please enter");
        }

        [Fact]
        public void CanFlattenForm()
        {
            IOUtility.EnsureAssets("pdfsharp-6.x\\pdfs\\DocumentWithAcroForm.pdf");
            var pdfPath = IOUtility.GetAssetsPath("pdfsharp-6.x\\pdfs")!;

            GlobalFontSettings.ResetAll();
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var sourceFile = Path.Combine(pdfPath, "DocumentWithAcroForm.pdf");
            var targetFile = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/Flattened.pdf");
            File.Copy(sourceFile, targetFile, true);

            var copiedDocument = PdfReader.Open(targetFile, PdfDocumentOpenMode.Modify);

            var fieldsInCopiedDocument = GetAllFields(copiedDocument);
            // fill all fields
            FillFields(fieldsInCopiedDocument);

            // flatten the form. after that, AcroForm should be null and all annotations should be removed
            // (this is true for the tested document, other documents may contain annotations not related to Form-Fields)
            copiedDocument.FlattenAcroForm();
            copiedDocument.AcroForm.Should().BeNull();
            copiedDocument.Pages[0].Annotations.Count.Should().Be(0);
            copiedDocument.Save(targetFile);
        }

        [Fact]
        public void CanFlattenCreatedForm()
        {
            var filePath = CanCreateNewForm();     // create the form

            GlobalFontSettings.ResetAll();
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            using var fs = File.OpenRead(filePath);
            var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);

            // import into new document
            var copiedDocument = new PdfDocument();
            foreach (var page in inputDocument.Pages)
                copiedDocument.AddPage(page);
            copiedDocument.ImportAcroForm(inputDocument.AcroForm!);

            copiedDocument.FlattenAcroForm();
            copiedDocument.AcroForm.Should().BeNull();
            copiedDocument.Pages[0].Annotations.Count.Should().Be(0);

            var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/CreatedForm_flattened.pdf");
            using var fsOut = File.Create(outFileName);
            copiedDocument.Save(fsOut);

            // don't know what to assert here, have to check with "real" eyes
            // (we mainly want to check the behavior of the AcroFieldRenderers)
        }

        [Fact]
        public string CanCreateNewForm()
        {
            // test some special characters
            // Note: Current limitations regarding CJK/Arabic, etc. still applies
            // make sure you use a font that supports the used characters !
            const string firstNameValue = "Sebastién";
            const string lastNameValue = "Süßölgefäß";  // yep, that's a valid german word

            GlobalFontSettings.ResetAll();
            // we use one of the 14 standard-fonts here (Helvetica)
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var document = new PdfDocument();
            var page1 = document.AddPage();
            var page2 = document.AddPage();
            var acroForm = document.GetOrCreateAcroForm();
            var textFont = new XFont(StandardFontNames.Helvetica, 12, XFontStyleEx.Regular,
                new XPdfFontOptions(PdfFontEncoding.Automatic, PdfFontEmbedding.EmbedCompleteFontFile));
            // use same font with different size
            var textFontBig = XFont.FromExisting(textFont, 24);

            acroForm.SetDefaultAppearance(textFont, 12, XColors.Black);

            double x = 40, y = 80;
            var page1Renderer = XGraphics.FromPdfPage(page1);
            var page2Renderer = XGraphics.FromPdfPage(page2);
            page1Renderer.DrawString("Name of Subject", textFont, XBrushes.Black, x, y);
            page2Renderer.DrawString("For Demo purposes. Modify the fields and observe the field on the first page is modified as well.",
                textFont, XBrushes.Black, x, y);

            y += 10;
            // Text fields
            var firstNameField = acroForm.AddTextField(field =>
            {
                // Note: Chromium-based browsers (ie. Edge/Chrome) do not render fields without a name
                field.Name = "FirstName";
                field.Font = textFontBig;
                field.ForeColor = XColors.DarkRed;
                field.Text = firstNameValue;
                // place annotation on both pages
                // if the document is opened in a PdfReader and one of the Annotations is changed (e.g. by typing inside it),
                // the other Annotation will be changed as well (as they belong to the same field)
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 200, 40)));
                });
                field.AddAnnotation(annot =>
                {
                    // Note: The border is currently always solid and 1 unit wide
                    annot.BorderColor = XColors.Green;
                    annot.BackColor = XColors.DarkGray;
                    // testing dynamic font-size by doubling the height of the second widget
                    annot.AddToPage(page2, new PdfRectangle(new XRect(x, y, 200, 40)));
                });
            });
            var lastNameField = acroForm.AddTextField(field =>
            {
                field.Name = "LastName";
                field.Font = textFont;
                field.Text = lastNameValue;
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x + 10 + 200, y, 100, 20)));
                });
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page2, new PdfRectangle(new XRect(x + 10 + 200, y, 100, 20)));
                });
            });

            y += 60;
            // Checkbox fields
            page1Renderer.DrawString("Subject's interests", textFont, XBrushes.Black, x, y);
            y += 10;
            var cbx1 = acroForm.AddCheckBoxField(field =>
            {
                field.Name = "Interest_cooking";
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 12, 12)));
                });
            });
            page1Renderer.DrawString("Cooking", textFont, XBrushes.Black, x + 20, y + 10);
            y += 20;
            var cbx2 = acroForm.AddCheckBoxField(field =>
            {
                field.Name = "Interest_coding";
                field.Checked = true;
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 12, 12)));
                });
            });
            page1Renderer.DrawString("Coding", textFont, XBrushes.Black, x + 20, y + 10);
            y += 20;
            var cbx3 = acroForm.AddCheckBoxField(field =>
            {
                field.Name = "Interest_cycling";
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 12, 12)));
                });
            });
            page1Renderer.DrawString("Cycling", textFont, XBrushes.Black, x + 20, y + 10);

            y += 40;
            // RadioButton fields
            page1Renderer.DrawString("Subject's gender", textFont, XBrushes.Black, x, y);
            y += 10;
            // used as parent-field for the radio-button (testing field-nesting)
            var groupGender = acroForm.AddGenericField(field =>
            {
                field.Name = "Group_Gender";
            });
            acroForm.AddRadioButtonField(field =>
            {
                field.Name = "Gender";
                // add individual buttons
                field.AddAnnotation("male", annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 12, 12)));
                });
                page1Renderer.DrawString("Male", textFont, XBrushes.Black, x + 20, y + 10);
                y += 20;
                field.AddAnnotation("female", annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 12, 12)));
                });
                page1Renderer.DrawString("Female", textFont, XBrushes.Black, x + 20, y + 10);
                y += 20;
                field.AddAnnotation("unspecified", annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 12, 12)));
                });
                page1Renderer.DrawString("Unspecified", textFont, XBrushes.Black, x + 20, y + 10);
                // as an alternative, you can also use field.SelectedIndex
                field.Value = "male";
                groupGender.AddChild(field);
            });

            y += 40;
            // ComboBox fields
            page1Renderer.DrawString("Select a number:", textFont, XBrushes.Black, x, y + 10);
            acroForm.AddComboBoxField(field =>
            {
                field.Name = "SelectedNumber";
                field.Options = ["One", "Two", "Three", "Four", "Five"];
                field.SelectedIndex = 2;    // select "Three"
                field.Font = textFont;
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x + 100, y, 100, 20)));
                });
            });

            y += 40;
            // ListBox fields
            page1Renderer.DrawString("Select a color:", textFont, XBrushes.Black, x, y + 10);
            acroForm.AddListBoxField(field =>
            {
                field.Name = "SelectedColor";
                field.Options = ["Blue", "Red", "Green", "Black", "White"];
                field.SelectedIndices = [1];    // select "Red"
                field.Font = textFont;
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x + 100, y, 100, 5 * textFont.Height)));
                });
            });

            y += 100;
            // PushButton fields
            acroForm.AddPushButtonField(button =>
            {
                button.Name = "SubmitButton";
                button.Caption = "Submit";
                button.Font = textFont;
                button.AddAnnotation(annot =>
                {
                    // TODO: these properties should be part of the field and propagated down to the annotations
                    annot.Highlighting = PdfAnnotationHighlightingMode.Invert;
                    annot.BorderColor = XColors.Gray;
                    annot.BackColor = XColors.LightBlue;
                    annot.Border.Width = 2;
                    annot.Border.BorderStyle = PdfAnnotationBorderStyle.Solid;
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 100, 20)));
                    // Note: setting icons for push-buttons is somewhat involved at the moment, but boils down to:
                    // - create an XForm
                    // - create XGraphics from the XForm
                    // - draw something using the graphics (like an image but it can be anything)
                    // - set the annotation's icon to the XForm's PdfForm, e.g. annot.NormalIcon = xform.PdfForm
                    // maybe we simplify this in the future if there is strong denand for it
                });
            });

            y += 40;
            // Signature fields
            acroForm.AddSignatureField(field =>
            {
                field.Name = "Signature";
                field.AddAnnotation(annot =>
                {
                    annot.BackColor = XColors.White;
                    annot.BorderColor = XColors.Gray;
                    annot.Border.Width = 1;
                    annot.Border.BorderStyle = PdfAnnotationBorderStyle.Solid;
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 160, 60)));
                });
            });

            var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/CreatedForm.pdf");
            using var fsOut = File.Create(outFileName);
            document.Save(fsOut, true);
            fsOut.Close();

            // read back and validate
            document = PdfReader.Open(outFileName, PdfDocumentOpenMode.Modify);
            var fields = GetAllFields(document);

            fields.Count.Should().Be(11);

            var field = fields.First(f => f.FullyQualifiedName == "FirstName");
            field.GetType().Should().Be(typeof(PdfTextField));
            ((PdfTextField)field).Text.Should().Be(firstNameValue);
            field.ForeColor.Should().Be(XColors.DarkRed);
            field.Font.Should().NotBeNull();
            field.Font!.Size.Should().Be(textFontBig.Size);
            field.Annotations.Elements.Count.Should().Be(2);
            field.Annotations.Elements[1].BorderColor.Should().Be(XColors.Green);
            field.Annotations.Elements[1].BackColor.Should().Be(XColors.DarkGray);

            field = fields.First(f => f.FullyQualifiedName == "LastName");
            field.GetType().Should().Be(typeof(PdfTextField));
            field.Font.Should().NotBeNull();
            field.Font!.Size.Should().Be(textFont.Size);
            ((PdfTextField)field).Text.Should().Be(lastNameValue);
            field.Annotations.Elements.Count.Should().Be(2);

            field = fields.First(f => f.FullyQualifiedName == "Interest_cooking");
            field.GetType().Should().Be(typeof(PdfCheckBoxField));
            ((PdfCheckBoxField)field).Checked.Should().Be(false);
            field.Annotations.Elements.Count.Should().Be(1);

            field = fields.First(f => f.FullyQualifiedName == "Interest_coding");
            field.GetType().Should().Be(typeof(PdfCheckBoxField));
            ((PdfCheckBoxField)field).Checked.Should().Be(true);
            field.Annotations.Elements.Count.Should().Be(1);

            field = fields.First(f => f.FullyQualifiedName == "Interest_cycling");
            field.GetType().Should().Be(typeof(PdfCheckBoxField));
            ((PdfCheckBoxField)field).Checked.Should().Be(false);
            field.Annotations.Elements.Count.Should().Be(1);

            field = fields.First(f => f.FullyQualifiedName == "Group_Gender");
            field.GetType().Should().Be(typeof(PdfGenericField));
            field.HasChildFields.Should().Be(true);
            field.Annotations.Elements.Count.Should().Be(0);
            field.Fields.Elements.Count.Should().Be(1);

            field = fields.First(f => f.FullyQualifiedName == "Group_Gender.Gender");
            field.GetType().Should().Be(typeof(PdfRadioButtonField));
            field.Annotations.Elements.Count.Should().Be(3);
            ((PdfRadioButtonField)field).SelectedIndex.Should().Be(0);
            ((PdfRadioButtonField)field).Options.Should().Equal(new[] { "male", "female", "unspecified" });
            ((PdfRadioButtonField)field).Value.Should().Be("male");

            field = fields.First(f => f.FullyQualifiedName == "SelectedNumber");
            field.GetType().Should().Be(typeof(PdfComboBoxField));
            field.Annotations.Elements.Count.Should().Be(1);
            ((PdfComboBoxField)field).SelectedIndex.Should().Be(2);
            ((PdfComboBoxField)field).Options.Should().Equal(new[] { "One", "Two", "Three", "Four", "Five" });
            ((PdfComboBoxField)field).Value.Should().Be("Three");

            field = fields.First(f => f.FullyQualifiedName == "SelectedColor");
            field.GetType().Should().Be(typeof(PdfListBoxField));
            field.Annotations.Elements.Count.Should().Be(1);
            ((PdfListBoxField)field).SelectedIndices.Count().Should().Be(1);
            ((PdfListBoxField)field).SelectedIndices.Should().Contain(1);
            ((PdfListBoxField)field).Options.Should().Equal(new[] { "Blue", "Red", "Green", "Black", "White" });
            ((PdfListBoxField)field).Value.Count().Should().Be(1);
            ((PdfListBoxField)field).Value.Should().Contain("Red");

            field = fields.First(f => f.FullyQualifiedName == "SubmitButton");
            field.GetType().Should().Be(typeof(PdfPushButtonField));
            field.Annotations.Elements.Count.Should().Be(1);
            ((PdfPushButtonField)field).Caption.Should().Be("Submit");
            field.Annotations.Elements.Count.Should().Be(1);
            field.Annotations.Elements[0].Border.Width.Should().Be(2);
            field.Annotations.Elements[0].Border.BorderStyle.Should().Be(PdfAnnotationBorderStyle.Solid);
            field.Annotations.Elements[0].Highlighting.Should().Be(PdfAnnotationHighlightingMode.Invert);
            field.Annotations.Elements[0].BorderColor.Should().Be(XColors.Gray);
            field.Annotations.Elements[0].BackColor.Should().Be(XColors.LightBlue);

            return outFileName;
        }

        [Fact]
        public void CanDeleteFields()
        {
            var filePath = CanCreateNewForm();

            var document = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);
            var startFields = document.AcroForm!.GetAllFields().ToList();
            var startAnnotsCount = document.Pages[0].Annotations.Elements.Count;
            foreach (var field in startFields)
            {
                if (field is PdfPushButtonField)
                    field.Remove();
                if (field is PdfSignatureField)
                    field.Remove();
            }
            var endFields = document.AcroForm!.GetAllFields().ToList();
            var endAnnotsCount = document.Pages[0].Annotations.Elements.Count;
            endFields.Count.Should().Be(startFields.Count - 2);
            endAnnotsCount.Should().Be(startAnnotsCount - 2);

            var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/AcroForms/CreatedForm_removed.pdf");
            document.Save(outFileName);
        }

        //[Fact(Skip = "Only run when we want it to run")]
        [Fact]
        public void RenderGlyphsOfStandardFonts()
        {
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            foreach (var fontName in StandardFontData.FontNames)
            {
                using var document = new PdfDocument();

                var renderFont = new XFont(fontName, 16);
                var helveticaFont = new XFont("Helvetica", 12);
                var headerFont = new XFont("Helvetica", 36);
                var brush = new XSolidBrush(XColors.Black);
                var left = 60.0;
                var top = 60.0;
                var bottom = 60.0;
                var gapX = 120.0;
                var gapY = 20.0;
                var x = left;
                var y = top;
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var fullFontName = renderFont.OpenTypeDescriptor.FontFace.FullFaceName;
                gfx.DrawString(fullFontName, headerFont, brush, x, y);
                y += 50;

                // test rendering a specific character
                //gfx.DrawString(char.ConvertFromUtf32(0x29C9C), renderFont, brush, x, y);
                //y += 40;

                var characterList = renderFont.GetSupportedCharacters();
                if (characterList.Count > 0)
                {
                    foreach (var c in characterList)
                    {
                        gfx.DrawString(c.ToString("X4"), helveticaFont, brush, x, y);
                        var s = char.ConvertFromUtf32(c);
                        gfx.DrawString(s, renderFont, brush, x + 80, y);
                        x += gapX;
                        if (x + gapX >= page.Width.Point)
                        {
                            x = left;
                            y += gapY;
                            if (y >= page.Height.Point - bottom)
                            {
                                gfx.Dispose();
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                x = left;
                                y = top;
                            }
                        }
                    }
                    gfx.Dispose();
                }
                else
                    output.WriteLine($"Font {fontName} has no glyphs");

                var outFileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/Fonts/StandardFonts/" + fontName);
                document.Save(outFileName);
            }
        }

        private static void FillFields(IList<PdfAcroField> fields)
        {
            foreach (var field in fields)
            {
                if (field.ReadOnly)
                    continue;
                // Values for the fields:
                // - TextFields: name of field
                // - CheckBoxes: checked
                // - RadioButtons: second option is checked (if there is only one option, this is checked)
                // - ChoiceFields (List, Combo): second option is selected (if there is only one option, this is selected)
                if (field is PdfTextField textField)
                    textField.Text = field.Name;
                else if (field is PdfComboBoxField comboBoxField)
                    comboBoxField.SelectedIndex = Math.Min(1, comboBoxField.Options.Count - 1);
                else if (field is PdfCheckBoxField checkboxField)
                    checkboxField.Checked = true;
                else if (field is PdfRadioButtonField radioButtonField)
                    radioButtonField.SelectedIndex = Math.Min(1, radioButtonField.Options.Count - 1);
                else if (field is PdfListBoxField listBoxField)
                    listBoxField.SelectedIndices = [Math.Min(1, listBoxField.Options.Count - 1)];
            }
        }

        private static List<PdfAcroField> GetAllFields(PdfDocument doc)
        {
            return (doc.AcroForm?.GetAllFields() ?? []).ToList();
        }

        /// <summary>
        /// Verifies, that the specified document has the expected field-values as set by <see cref="FillFields(IList{PdfAcroField})"/>
        /// </summary>
        /// <param name="documentFilePath"></param>
        private static void VerifyFieldsFilledWithDefaultValues(string documentFilePath)
        {
            VerifyFilledFields(documentFilePath, field =>
            {
                if (field is PdfTextField textField)
                {
                    textField.Text.Should().Be(textField.Name);
                }
                else if (field is PdfCheckBoxField checkBox)
                {
                    checkBox.Checked.Should().BeTrue();
                }
                else if (field is PdfRadioButtonField radioButton)
                {
                    radioButton.Options.Count.Should().BeGreaterThan(0);
                    radioButton.SelectedIndex.Should().Be(Math.Min(1, radioButton.Options.Count - 1));
                }
                else if (field is PdfComboBoxField comboBox)
                {
                    comboBox.SelectedIndex.Should().Be(Math.Min(1, comboBox.Options.Count - 1));
                }
                else if (field is PdfListBoxField listBox)
                {
                    listBox.Options.Count.Should().BeGreaterThan(0);
                    listBox.SelectedIndices.Count().Should().BeGreaterThan(0);
                    listBox.SelectedIndices.First().Should().Be(Math.Min(1, listBox.Options.Count - 1));
                }
            });
        }

        private static void VerifyFilledFields(string documentFilePath, Action<PdfAcroField> fieldVerifier)
        {
            using var fs = File.OpenRead(documentFilePath);
            var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Modify);
            var allFields = GetAllFields(inputDocument);
            foreach (var field in allFields)
            {
                if (field.ReadOnly)
                    continue;
                fieldVerifier(field);
            }
        }
    }
}
