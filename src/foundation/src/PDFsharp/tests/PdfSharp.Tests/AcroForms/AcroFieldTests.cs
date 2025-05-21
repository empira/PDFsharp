using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.StandardFonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Annotations.enums;
using Xunit;

namespace PdfSharp.Tests.AcroForms
{
    public class AcroFieldTests
    {
        [Fact]
        public void Added_Fields_Are_Present()
        {
            var doc = new PdfDocument();
            var acroForm = doc.GetOrCreateAcroForm();

            acroForm.Should().NotBeNull();
            acroForm.Fields.Elements.Count.Should().Be(0);
            acroForm.AddTextField(field =>
            {
                field.Name = "text";
            });
            acroForm.Fields.Elements.Count.Should().Be(1);
        }

        [Fact]
        public void TextFieldValue()
        {
            var doc = CreateTestDocument();
            var field = GetAllFields(doc).FirstOrDefault(f => f.FullyQualifiedName == "FirstName");
            field.Should().NotBeNull();
            field?.GetType().Should().Be<PdfTextField>();
            var textField = field as PdfTextField;
            textField.Should().NotBeNull();
            if (textField != null)
            {
                textField.Value.Should().BeEmpty();
                textField.Text.Should().BeEmpty();

                const string theValue = "TestText";
                textField.Text = theValue;

                textField.Text.Should().Be(theValue);
                textField.Value.ToString().Should().Be(theValue);

                // reset to null
                textField.Text = null!;

                textField.Text?.Should().BeEmpty();
                textField.Value.Should().BeEmpty();
            }
        }

        [Fact]
        public void CheckBoxValue()
        {
            var doc = CreateTestDocument();
            var field = GetAllFields(doc).FirstOrDefault(f => f.FullyQualifiedName == "Interest_cooking");
            field.Should().NotBeNull();
            field?.GetType().Should().Be<PdfCheckBoxField>();
            var checkBoxField = field as PdfCheckBoxField;
            checkBoxField.Should().NotBeNull();
            if (checkBoxField != null)
            {
                checkBoxField.Checked.Should().BeFalse();
                checkBoxField.Value.Should().Be("Off");

                checkBoxField.Checked = true;

                checkBoxField.Checked.Should().BeTrue();
                checkBoxField.Value.Should().Be("Yes");

                checkBoxField.Checked = false;

                checkBoxField.Checked.Should().BeFalse();
                checkBoxField.Value.Should().Be("Off");

                checkBoxField.Value = "Yes";
                checkBoxField.Checked.Should().BeTrue();

                checkBoxField.Value = "Off";
                checkBoxField.Checked.Should().BeFalse();

                Action act = () => checkBoxField.Value = "Nope";   // invalid value
                act.Should().Throw<ArgumentException>();
            }
        }

        [Fact]
        public void RadioButtonValue()
        {
            var doc = CreateTestDocument();
            var field = GetAllFields(doc).FirstOrDefault(f => f.FullyQualifiedName == "Group_Gender.Gender");
            field.Should().NotBeNull();
            field?.GetType().Should().Be<PdfRadioButtonField>();
            var radioButtonField = field as PdfRadioButtonField;
            radioButtonField.Should().NotBeNull();
            if (radioButtonField != null)
            {
                radioButtonField.Value.Should().Be("Off");
                radioButtonField.SelectedIndex.Should().Be(-1);
                radioButtonField.Options.Should().Equal(new[] { "male", "female", "unspecified" });
                radioButtonField.ExportValues.Should().Equal(new[] { "male", "female", "unspecified" });

                radioButtonField.SelectedIndex = 0;
                radioButtonField.SelectedIndex.Should().Be(0);
                radioButtonField.Value?.Should().Be("male");

                radioButtonField.SelectedIndex = 1;
                radioButtonField.SelectedIndex.Should().Be(1);
                radioButtonField.Value?.Should().Be("female");

                radioButtonField.SelectedIndex = 2;
                radioButtonField.SelectedIndex.Should().Be(2);
                radioButtonField.Value?.Should().Be("unspecified");

                radioButtonField.SelectedIndex = -1;
                radioButtonField.SelectedIndex.Should().Be(-1);
                radioButtonField.Value?.Should().Be("Off");

                radioButtonField.Value = "male";
                radioButtonField.SelectedIndex.Should().Be(0);
                radioButtonField.Value?.Should().Be("male");

                radioButtonField.Value = "female";
                radioButtonField.SelectedIndex.Should().Be(1);
                radioButtonField.Value?.Should().Be("female");

                radioButtonField.Value = "unspecified";
                radioButtonField.SelectedIndex.Should().Be(2);
                radioButtonField.Value?.Should().Be("unspecified");

                radioButtonField.Value = null!;
                radioButtonField.SelectedIndex.Should().Be(-1);
                radioButtonField.Value?.Should().Be("Off");

                Action act = () => radioButtonField.Value = "Nope";     // invalid value
                act.Should().Throw<ArgumentException>();

                act = () => radioButtonField.SelectedIndex = 10;
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void ComboBoxValue()
        {
            var doc = CreateTestDocument();
            var field = GetAllFields(doc).FirstOrDefault(f => f.FullyQualifiedName == "SelectedNumber");
            field.Should().NotBeNull();
            field?.GetType().Should().Be<PdfComboBoxField>();
            var comboBoxField = field as PdfComboBoxField;
            comboBoxField.Should().NotBeNull();
            if (comboBoxField != null)
            {
                comboBoxField.SelectedIndex.Should().Be(-1);
                comboBoxField.Value.Should().BeEmpty();
                comboBoxField.Options.Should().Equal(new[] { "One", "Two", "Three", "Four", "Five" });

                comboBoxField.SelectedIndex = 0;
                comboBoxField.SelectedIndex.Should().Be(0);
                comboBoxField.Value.Should().Be("One");

                comboBoxField.SelectedIndex = 4;
                comboBoxField.SelectedIndex.Should().Be(4);
                comboBoxField.Value.Should().Be("Five");

                comboBoxField.Value = "One";
                comboBoxField.SelectedIndex.Should().Be(0);
                comboBoxField.Value.Should().Be("One");

                comboBoxField.Value = "Five";
                comboBoxField.SelectedIndex.Should().Be(4);
                comboBoxField.Value.Should().Be("Five");

                comboBoxField.SelectedIndex = -1;
                comboBoxField.SelectedIndex.Should().Be(-1);
                comboBoxField.Value.Should().BeEmpty();

                // invalid value
                Action act = () => comboBoxField.Value = "Ten";
                act.Should().Throw<ArgumentException>();

                act = () => comboBoxField.SelectedIndex = 10;
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        [Fact]
        public void ListBoxValue()
        {
            var doc = CreateTestDocument();
            var field = GetAllFields(doc).FirstOrDefault(f => f.FullyQualifiedName == "SelectedColor");
            field.Should().NotBeNull();
            field?.GetType().Should().Be<PdfListBoxField>();
            var listBoxField = field as PdfListBoxField;
            listBoxField.Should().NotBeNull();
            if (listBoxField != null)
            {
                listBoxField.SelectedIndices.Should().BeEmpty();
                listBoxField.Value.Should().BeEmpty();
                listBoxField.Options.Should().Equal(new[] { "Blue", "Red", "Green", "Black", "White" });

                listBoxField.SelectedIndices = new[] { 0 };
                listBoxField.SelectedIndices.Should().Equal(new[] { 0 });
                listBoxField.Value.Should().Equal(new[] { "Blue" });

                listBoxField.SelectedIndices = new[] { 0, 4 };
                listBoxField.SelectedIndices.Should().Equal(new[] { 0, 4 });
                listBoxField.Value.Should().Equal(new[] { "Blue", "White" });

                listBoxField.SelectedIndices = new[] { 1, 3 };
                listBoxField.SelectedIndices.Should().Equal(new[] { 1, 3 });
                listBoxField.Value.Should().Equal(new[] { "Red", "Black" });

                listBoxField.Value = Array.Empty<string>();
                listBoxField.SelectedIndices.Should().BeEmpty();
                listBoxField.Value.Should().BeEmpty();

                listBoxField.Value = new[] { "Green", "Blue" };
                listBoxField.SelectedIndices.Should().Equal(new[] { 0, 2 });
                listBoxField.Value.Should().Equal(new[] { "Blue", "Green" });

                listBoxField.Value = null!;
                listBoxField.SelectedIndices.Should().BeEmpty();
                listBoxField.Value.Should().BeEmpty();

                listBoxField.Value = new[] { "Green" };
                listBoxField.SelectedIndices.Should().Equal(new[] { 2 });
                listBoxField.Value.Should().Equal(new[] { "Green" });

                listBoxField.SelectedIndices = null!;
                listBoxField.SelectedIndices.Should().BeEmpty();
                listBoxField.Value.Should().BeEmpty();

                Action act = () => listBoxField.SelectedIndices = new[] { 1, 10 };  // invalid index
                act.Should().Throw<ArgumentOutOfRangeException>();

                act = () => listBoxField.Value = new[] { "Red", "Orange" };     // Orange does not exist
                act.Should().Throw<ArgumentException>();
            }
        }

        private static PdfDocument CreateTestDocument()
        {
            // code copied from AcroFormsTests with minimal modifications

            // we use one of the 14 standard-fonts here (Helvetica)
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var document = new PdfDocument();
            var page1 = document.AddPage();
            var page2 = document.AddPage();
            var acroForm = document.GetOrCreateAcroForm();
            var textFont = new XFont(StandardFontNames.Helvetica, 12, XFontStyleEx.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.EmbedCompleteFontFile));

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
                field.Font = textFont;
                field.ForeColor = XColors.DarkRed;
                //field.Text = string.Empty;
                // place annotation on both pages
                // if the document is opened in a PdfReader and one of the Annotations is changed (e.g. by typing inside it),
                // the other Annotation will be changed as well (as they belong to the same field)
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x, y, 100, 20)));
                });
                field.AddAnnotation(annot =>
                {
                    // Note: The border is currently always solid and 1 unit wide
                    annot.BorderColor = XColors.Green;
                    annot.BackColor = XColors.DarkGray;
                    annot.AddToPage(page2, new PdfRectangle(new XRect(x, y, 100, 20)));
                });
            });
            var lastNameField = acroForm.AddTextField(field =>
            {
                field.Name = "LastName";
                field.Font = textFont;
                //field.Text = string.Empty;
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x + 10 + 100, y, 100, 20)));
                });
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page2, new PdfRectangle(new XRect(x + 10 + 100, y, 100, 20)));
                });
            });

            y += 40;
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
                //field.Checked = true;
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
                //field.Value = "male";
                groupGender.AddChild(field);
            });

            y += 40;
            // ComboBox fields
            page1Renderer.DrawString("Select a number:", textFont, XBrushes.Black, x, y + 10);
            acroForm.AddComboBoxField(field =>
            {
                field.Name = "SelectedNumber";
                field.Options = new[] { "One", "Two", "Three", "Four", "Five" };
                //field.SelectedIndex = 2;    // select "Three"
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
                field.Options = new[] { "Blue", "Red", "Green", "Black", "White" };
                //field.SelectedIndices = new[] { 1 };    // select "Red"
                field.Font = textFont;
                field.AddAnnotation(annot =>
                {
                    annot.AddToPage(page1, new PdfRectangle(new XRect(x + 100, y, 100, 5 * textFont.Height)));
                });
            });

            y += 100;
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
                });
            });
            // TODO: Signature fields

            return document;
        }

        private static IEnumerable<PdfAcroField> GetAllFields(PdfDocument doc)
        {
            return doc.AcroForm?.GetAllFields() ?? Array.Empty<PdfAcroField>();
        }
    }
}