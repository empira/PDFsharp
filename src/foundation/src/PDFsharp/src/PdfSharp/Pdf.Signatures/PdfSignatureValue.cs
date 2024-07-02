using PdfSharp.Internal;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// Defines the value for a <see cref="PdfSignatureField"/>
    /// </summary>
    public class PdfSignatureValue : PdfDictionary
    {
        /// <summary>
        /// Used to report the positions of the values of <see cref="Contents"/> and <see cref="ByteRange"/>
        /// when writing this field to a stream
        /// </summary>
        /// <param name="signatureValue">A reference to the value itself</param>
        /// <param name="start">The start-position of the value</param>
        /// <param name="end">The end-position of the value</param>
        internal delegate void SignatureWriteCallback(PdfSignatureValue signatureValue, SizeType start, SizeType end);

        internal SignatureWriteCallback? SignatureContentsWritten;

        internal SignatureWriteCallback? SignatureRangeWritten;

        internal PdfSignatureValue(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Sig");
        }

        internal PdfSignatureValue(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// (Required; inheritable) The name of the signature handler to be used for
        /// authenticating the field’s contents, such as Adobe.PPKLite, Entrust.PPKEF,
        /// CICI.SignIt, or VeriSign.PPKVS.
        /// </summary>
        public string Filter
        {
            get
            {
                var val = Elements.GetName(Keys.Filter);
                return val;
            }
            set
            {
                Elements.SetName(Keys.Filter, value);
            }
        }

        /// <summary>
        /// (Optional) A name that describes the encoding of the signature value and key 
        /// information in the signature dictionary.<br></br>
        /// A PDF processor may use any handler that supports this format to validate the signature.
        /// </summary>
        public string SubFilter
        {
            get
            {
                var val = Elements.GetName(Keys.SubFilter);
                return val;
            }
            set
            {
                Elements.SetName(Keys.SubFilter, value);
            }
        }

        /// <summary>
        /// (Optional) The name of the person or authority signing the document.
        /// </summary>
        public string Name
        {
            get
            {
                var val = Elements.GetString(Keys.Name);
                return val;
            }
            set
            {
                Elements.SetString(Keys.Name, value);
            }
        }

        /// <summary>
        /// (Optional) The CPU host name or physical location of the signing.
        /// </summary>
        public string Location
        {
            get
            {
                var val = Elements.GetString(Keys.Location);
                return val;
            }
            set
            {
                Elements.SetString(Keys.Location, value);
            }
        }

        /// <summary>
        /// (Optional) The reason for the signing, such as (I agree…).
        /// </summary>
        public string Reason
        {
            get
            {
                var val = Elements.GetString(Keys.Reason);
                return val;
            }
            set
            {
                Elements.SetString(Keys.Reason, value);
            }
        }

        /// <summary>
        /// (Optional) Information provided by the signer to enable a recipient to contact the signer to verify the signature.<br></br>
        /// If SubFilter is ETSI.RFC3161, this entry should not be used and should be ignored by a PDF processor.
        /// </summary>
        public string ContactInfo
        {
            get
            {
                var val = Elements.GetString(Keys.ContactInfo);
                return val;
            }
            set
            {
                Elements.SetString(Keys.ContactInfo, value);
            }
        }

        /// <summary>
        /// (Optional) The time of signing.<br></br>
        /// Depending on the signature handler, this may be a normal unverified computer time
        /// or a time generated in a verifiable way from a secure time server.
        /// </summary>
        public DateTime SigningDate
        {
            get
            {
                var dt = Elements.GetDateTime(Keys.M, DateTime.UtcNow);
                return dt;
            }
            set
            {
                Elements.SetDateTime(Keys.M, value);
            }
        }

        /// <summary>
        /// (Required) An array of pairs of integers (starting byte offset, length in bytes)
        /// describing the exact byte range for the digest calculation.<br></br>
        /// Multiple discontinuous byte ranges may be used to describe a digest that does not include the
        /// signature token itself.
        /// </summary>
        public PdfArray? ByteRange
        {
            get
            {
                return Elements.GetArray(Keys.ByteRange);
            }
            set
            {
                if (value is not null)
                    Elements.SetObject(Keys.ByteRange, value);
                else
                    Elements.Remove(Keys.ByteRange);
            }
        }

        /// <summary>
        /// (Required) The encrypted signature token.
        /// </summary>
        public byte[] Contents
        {
            get
            {
                var str = Elements.GetString(Keys.Contents);
                return PdfEncoders.RawEncoding.GetBytes(str);
            }
            set
            {
                var str = PdfEncoders.RawEncoding.GetString(value, 0, value.Length);
                var hexStr = new PdfString(str, PdfStringFlags.HexLiteral);
                Elements[Keys.Contents] = hexStr;
            }
        }

        /// <summary>
        /// Writes a key/value pair of this signature field dictionary.
        /// </summary>
        internal override void WriteDictionaryElement(PdfWriter writer, PdfName key)
        {
            // Don’t encrypt Contents key’s value (PDF Reference 2.0: 7.6.2, Page 71).
            if (key.Value == Keys.Contents)
            {
                var item = Elements[key];
                key.WriteObject(writer);
                var start = writer.Position;
                item?.WriteObject(writer);
                var end = writer.Position;
                writer.NewLine();
                SignatureContentsWritten?.Invoke(this, start, end);

                //var effectiveSecurityHandler = writer.EffectiveSecurityHandler;
                //writer.EffectiveSecurityHandler = null;
                //base.WriteDictionaryElement(writer, key);
                //writer.EffectiveSecurityHandler = effectiveSecurityHandler;
            }
            else if (key.Value == Keys.ByteRange)
            {
                var item = Elements[key];
                key.WriteObject(writer);
                var start = writer.Position;
                item?.WriteObject(writer);
                var end = writer.Position;
                writer.NewLine();
                SignatureRangeWritten?.Invoke(this, start, end);
            }
            else
                base.WriteDictionaryElement(writer, key);
        }

        /// <summary>
        /// Predefined keys of this dictionary.<br></br>
        /// PDF Reference 2.0, Chapter 12.8.1, Table 255<br></br>
        /// Consult the spec for more detailed information.
        /// </summary>
        public class Keys : KeysBase
        {
            /// <summary>
            /// (Optional if Sig; Required if DocTimeStamp)<br></br>
            /// The type of PDF object that this dictionary describes; if present, shall be Sig for a signature dictionary or
            /// DocTimeStamp for a timestamp signature dictionary.<br></br>
            /// The default value is: Sig.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

            /// <summary>
            /// (Required; inheritable) The name of the signature handler to be used for
            /// authenticating the field’s contents, such as Adobe.PPKLite, Entrust.PPKEF,
            /// CICI.SignIt, or VeriSign.PPKVS.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string Filter = "/Filter";

            /// <summary>
            /// (Optional) A name that describes the encoding of the signature value and key 
            /// information in the signature dictionary.<br></br>
            /// A PDF processor may use any handler that supports this format to validate the signature.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string SubFilter = "/SubFilter";

            /// <summary>
            /// (Required) An array of pairs of integers (starting byte offset, length in bytes)
            /// describing the exact byte range for the digest calculation.<br></br>
            /// Multiple discontinuous byte ranges may be used to describe a digest that does not include the
            /// signature token itself.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string ByteRange = "/ByteRange";

            /// <summary>
            /// (Required) The encrypted signature token.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Required)]
            public const string Contents = "/Contents";

            // Cert (deprecated ?)

            /// <summary>
            /// (Optional; PDF 1.5) An array of signature reference dictionaries 
            /// (see "Table 256 — Entries in a signature reference dictionary").<br></br>
            /// If SubFilter is ETSI.RFC3161, this entry shall not be used.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Reference = "/Reference";

            /// <summary>
            /// (Optional) An array of three integers that shall specify changes to the 
            /// document that have been made between the previous signature and this 
            /// signature: in this order, the number of pages altered, the number of fields altered,
            /// and the number of fields filled in.<br></br>
            /// The ordering of signatures shall be determined by the value of ByteRange.<br></br>
            /// Since each signature results in an incremental save, later signatures have a
            /// greater length value.<br></br>
            /// If SubFilter is ETSI.RFC3161, this entry shall not be used.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Changes = "/Changes";

            /// <summary>
            /// (Optional) The name of the person or authority signing the document.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Name ="/Name";

            /// <summary>
            /// (Optional) The time of signing. Depending on the signature handler, this
            /// may be a normal unverified computer time or a time generated in a verifiable
            /// way from a secure time server.
            /// </summary>
            [KeyInfo(KeyType.Date | KeyType.Optional)]
            public const string M = "/M";

            /// <summary>
            /// (Optional) The CPU host name or physical location of the signing.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Location = "/Location";

            /// <summary>
            /// (Optional) The reason for the signing, such as (I agree…).
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Reason = "/Reason";

            /// <summary>
            /// (Optional) Information provided by the signer to enable a recipient to contact the signer to verify the signature.<br></br>
            /// If SubFilter is ETSI.RFC3161, this entry should not be used and should be ignored by a PDF processor.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string ContactInfo = "/ContactInfo";

            /// <summary>
            /// (Optional; deprecated in PDF 2.0) The version of the signature handler that 
            /// was used to create the signature.<br></br>
            /// (PDF 1.5) This entry shall not be used, and the information shall be stored in the Prop_Build dictionary.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string R = "/R";

            /// <summary>
            /// (Optional; PDF 1.5) The version of the signature dictionary format.<br></br>
            /// It corresponds to the usage of the signature dictionary in the context of the value of SubFilter.<br></br>
            /// The value is 1 if the Reference dictionary shall be considered critical to the validation of the signature.<br></br>
            /// If SubFilter is ETSI.RFC3161, this V value shall be 0 (possibly by default).<br></br>
            /// Default value: 0. 
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string V = "/V";

            /// <summary>
            /// (Optional; PDF 1.5) A dictionary that may be used by a signature handler to 
            /// record information that captures the state of the computer environment used 
            /// for signing, such as the name of the handler used to create the signature,
            /// software build date, version, and operating system.<br></br>
            /// The use of this dictionary is defined by Adobe PDF Signature Build Dictionary
            /// Specification, which provides implementation guidelines. 
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Prop_Build = "/Prop_Build";

            /// <summary>
            /// (Optional; PDF 1.5) The number of seconds since the signer was last 
            /// authenticated, used in claims of signature repudiation.<br></br>
            /// It should be omitted if the value is unknown.<br></br>
            /// If SubFilter is ETSI.RFC3161, this entry shall not be used.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Prop_AuthTime = "/Prop_AuthTime";

            /// <summary>
            /// (Optional; PDF 1.5) The method that shall be used to authenticate the signer, 
            /// used in claims of signature repudiation.<br></br>
            /// Valid values shall be PIN, Password, and Fingerprint.<br></br>
            ///If SubFilter is ETSI.RFC3161, this entry shall not be used.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Prop_AuthType = "/Prop_AuthType";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
