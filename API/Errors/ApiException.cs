namespace API.Errors
{
    public class ApiException
    {
        
        #region Public Properties
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }            
        #endregion

        public ApiException(int statusCode, string message = null, string details = null)
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Details = details;

        }

    }
}