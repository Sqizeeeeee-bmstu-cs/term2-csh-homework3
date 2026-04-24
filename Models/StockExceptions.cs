

namespace homework3.Exceptions;


public class StockAppException : Exception 
{
    public StockAppException(string message) : base(message) { }
}

public class ApiException : StockAppException 
{
    public ApiException(string message) : base(message) { }
}

public class ValidationException : StockAppException 
{
    public ValidationException(string message) : base(message) { }
}

public class UnauthorizedException : StockAppException 
{
    public UnauthorizedException(string message) : base(message) { }
}

