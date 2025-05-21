﻿namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Contains the renderers for <see cref="PdfAcroField"/>s<br></br>
    /// Individual renderers may be overriden by their respective implementations
    /// </summary>
    public class PdfAcroFieldRenderer
    {
        private PdfCheckBoxFieldRenderer checkBoxFieldRenderer = new();
        private PdfComboBoxFieldRenderer comboBoxFieldRenderer = new();
        private PdfListBoxFieldRenderer listBoxFieldRenderer = new();
        private PdfPushButtonFieldRenderer pushButtonFieldRenderer = new();
        private PdfRadioButtonFieldRenderer radioButtonFieldRenderer = new();
        private PdfSignatureFieldRenderer signatureFieldRenderer = new();
        private PdfTextFieldRenderer textFieldRenderer = new();

        /// <summary>
        /// Gets or sets the renderer for <see cref="PdfCheckBoxField"/>s
        /// <br></br>Trying to set this to null will throw an Exception
        /// </summary>
        public PdfCheckBoxFieldRenderer CheckBoxFieldRenderer
        {
            get => checkBoxFieldRenderer;
            set => checkBoxFieldRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the renderer for <see cref="PdfComboBoxField"/>s
        /// <br></br>Trying to set this to null will throw an Exception
        /// </summary>
        public PdfComboBoxFieldRenderer ComboBoxFieldRenderer
        {
            get => comboBoxFieldRenderer;
            set => comboBoxFieldRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the renderer for <see cref="PdfListBoxField"/>s
        /// <br></br>Trying to set this to null will throw an Exception
        /// </summary>
        public PdfListBoxFieldRenderer ListBoxFieldRenderer
        {
            get => listBoxFieldRenderer;
            set => listBoxFieldRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the renderer for <see cref="PdfPushButtonField"/>s
        /// <br></br>Trying to set this to null will throw an Exception
        /// </summary>
        public PdfPushButtonFieldRenderer PushButtonFieldRenderer
        {
            get => pushButtonFieldRenderer;
            set => pushButtonFieldRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the renderer for <see cref="PdfRadioButtonField"/>s
        /// <br></br>Trying to set this to null will throw an Exception
        /// </summary>
        public PdfRadioButtonFieldRenderer RadioButtonFieldRenderer
        {
            get => radioButtonFieldRenderer;
            set => radioButtonFieldRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the renderer for <see cref="PdfSignatureField"/>s
        /// <br></br>Trying to set this to null will throw an Exception
        /// </summary>
        public PdfSignatureFieldRenderer SignatureFieldRenderer
        {
            get => signatureFieldRenderer;
            set => signatureFieldRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the renderer for <see cref="PdfTextField"/>s
        /// <br></br>Trying to set this to null will throw an Exception
        /// </summary>
        public PdfTextFieldRenderer TextFieldRenderer
        {
            get => textFieldRenderer;
            set => textFieldRenderer = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
