namespace backend.Common
{
  public class ApiResponse<T>
  {
    public bool Success { get; set; }  // Para indicar se a operação foi bem-sucedida
    public T Data { get; set; }        // Dados retornados pela operação
    public string Error { get; set; }  // Mensagens de erro, se houver

    // Construtores para sucesso e erro
    public ApiResponse(T data)
    {
      Success = true;
      Data = data;
    }

    public ApiResponse(string error)
    {
      Success = false;
      Error = error;
    }
  }
}