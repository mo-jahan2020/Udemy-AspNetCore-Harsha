namespace Exceptions
{
 // تعریف یک رسیدگی به استثناء اختصاصی
    public class InvalidPersonIDException : ArgumentException
 {
  public InvalidPersonIDException() : base()
  {
  }

  public InvalidPersonIDException(string? message) : base(message)
  {
  }

  public InvalidPersonIDException(string? message, Exception? innerException)
  {
  }
 }
}