using System;
using System.Collections.Generic;
using System.Linq;
using Orient.Client.Protocol;

namespace Orient.Client
{
    public class OCommandResult
    {
        private readonly ODocument _document;

        internal OCommandResult(ODocument document)
        {
            _document = document;
        }

        public int GetScalarResult()
        {
            switch (_document.GetField<PayloadStatus>("PayloadStatus"))
            {
                case PayloadStatus.SingleRecord:
                    return 1;

                case PayloadStatus.RecordCollection:
                    return _document.GetField<List<ODocument>>("Content").Count;

                case PayloadStatus.SerializedResult:
                    int intResult;
                    bool boolResult;
                    var content = _document.GetField<string>("Content");
                    if (int.TryParse(content, out intResult))
                        return intResult;
                    if (bool.TryParse(content, out boolResult))
                        return Convert.ToInt32(boolResult);
                    break;
            }

            return 0;
        }

        public ODocument ToSingle()
        {
            ODocument document = null;

            switch (_document.GetField<PayloadStatus>("PayloadStatus"))
            {
                case PayloadStatus.SingleRecord:
                    document = _document.GetField<ODocument>("Content");
                    break;

                case PayloadStatus.RecordCollection:
                    document = _document.GetField<List<ODocument>>("Content").FirstOrDefault();
                    break;

                case PayloadStatus.SerializedResult:
                    document = new ODocument();
                    document.SetField("value", _document.GetField<string>("Content"));
                    break;
            }

            return document;
        }

        public List<ODocument> ToList()
        {
            return _document.GetField<List<ODocument>>("Content");
        }

        public ODocument ToDocument()
        {
            return _document;
        }
    }
}
