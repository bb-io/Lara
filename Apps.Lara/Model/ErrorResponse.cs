using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Lara.Model;
public class ErrorResponse
{
    public int Status { get; set; }
    public Error Error { get; set; }
}

public class Error
{
    public string Type { get; set; }
    public string Message { get; set; }
}