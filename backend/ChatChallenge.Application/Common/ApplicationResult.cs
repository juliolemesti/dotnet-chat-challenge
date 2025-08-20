namespace ChatChallenge.Application.Common;

public class ApplicationResult<T>
{
  public bool IsSuccess { get; set; }
  public T? Data { get; set; }
  public string ErrorMessage { get; set; } = string.Empty;
  public string ErrorCode { get; set; } = string.Empty;

  public static ApplicationResult<T> Success(T data)
  {
    return new ApplicationResult<T>
    {
      IsSuccess = true,
      Data = data
    };
  }

  public static ApplicationResult<T> Failure(string errorMessage, string errorCode = "")
  {
    return new ApplicationResult<T>
    {
      IsSuccess = false,
      ErrorMessage = errorMessage,
      ErrorCode = errorCode
    };
  }
}

public class ApplicationResult
{
  public bool IsSuccess { get; set; }
  public string ErrorMessage { get; set; } = string.Empty;
  public string ErrorCode { get; set; } = string.Empty;

  public static ApplicationResult Success()
  {
    return new ApplicationResult { IsSuccess = true };
  }

  public static ApplicationResult Failure(string errorMessage, string errorCode = "")
  {
    return new ApplicationResult
    {
      IsSuccess = false,
      ErrorMessage = errorMessage,
      ErrorCode = errorCode
    };
  }
}
