namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Common response
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// Types of error
        /// </summary>
        public enum ErrorCodeType
        {
            /// <summary>
            /// Input field is invalid
            /// </summary>
            InvalidInputField = 0
        }

        /// <summary>
        /// Error response
        /// </summary>
        public class ErrorModel
        {
            /// <summary>
            /// Type of error
            /// </summary>
            public ErrorCodeType Code { get; set; }
            /// <summary>
            /// Associated field
            /// </summary>
            public string Field { get; set; }
            /// <summary>
            /// Error message
            /// </summary>
            public string Message { get; set; }
        }

        /// <summary>
        /// Error
        /// </summary>
        public ErrorModel Error { get; set; }

        public static ResponseModel CreateInvalidFieldError(string field, string message)
        {
            return new ResponseModel
            {
                Error = new ErrorModel
                {
                    Code = ErrorCodeType.InvalidInputField,
                    Field = field,
                    Message = message
                }
            };
        }

        public static ResponseModel CreateFail(ErrorCodeType errorCodeType, string message)
        {
            return new ResponseModel
            {
                Error = new ErrorModel
                {
                    Code = errorCodeType,
                    Message = message
                }
            };
        }

        private static readonly ResponseModel OkInstance = new ResponseModel();

        public static ResponseModel CreateOk()
        {
            return OkInstance;
        }
    }

    /// <summary>
    /// Common response with result data
    /// </summary>
    public class ResponseModel<T> : ResponseModel
    {
        /// <summary>
        /// Result
        /// </summary>
        public T Result { get; set; }

        public static ResponseModel<T> CreateOk(T result)
        {
            return new ResponseModel<T>
            {
                Result = result
            };
        }

        public new static ResponseModel<T> CreateInvalidFieldError(string field, string message)
        {
            return new ResponseModel<T>
            {
                Error = new ErrorModel
                {
                    Code = ErrorCodeType.InvalidInputField,
                    Field = field,
                    Message = message
                }
            };
        }

        public new static ResponseModel<T> CreateFail(ErrorCodeType errorCodeType, string message)
        {
            return new ResponseModel<T>
            {
                Error = new ErrorModel
                {
                    Code = errorCodeType,
                    Message = message
                }
            };
        }
    }
}
