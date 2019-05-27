namespace Galvarino.Web.Models.Exception
{
    [System.Serializable]
    public class TareaNoEnEtapaException : System.Exception
    {
        public TareaNoEnEtapaException() { }
        public TareaNoEnEtapaException(string message) : base(message) { }
        public TareaNoEnEtapaException(string message, System.Exception inner) : base(message, inner) { }
        protected TareaNoEnEtapaException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}