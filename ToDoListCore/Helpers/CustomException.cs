namespace ToDoListCore.Helpers
{
    public class CustomException : Exception
    {
        public List<CustomError> Errors { get; }

        public CustomError LastError => Errors.LastOrDefault();

        public int LastCustomErrorCode => LastError?.Code ?? 700;

        public string LastErrorMessage => LastError?.Message;

        public CustomException(int code, string message, params object[] args)
        {
            Errors ??= new List<CustomError>();

            Errors.Add(new CustomError
            {
                Code = code,
                Message = string.Format(message, args)
            });
        }

        public CustomException(int code, string message)
        {
            Errors ??= new List<CustomError>();

            Errors.Add(new CustomError
            {
                Code = code,
                Message = message
            });
        }
        public CustomException(IEnumerable<CustomError> errors)
        {
            Errors ??= new List<CustomError>();
            Errors.AddRange(errors);
        }


        //public CustomError CustomError { get; }
        //public CustomException(int customCode, string message, params object[] args)
        //{
        //    CustomError = new CustomError
        //    {
        //        code = customCode,
        //        Message = String.Format(message, args)
        //    };
        //}
    }
}
