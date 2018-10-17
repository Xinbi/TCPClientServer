using System;
using Newtonsoft.Json;

namespace Common
{
    public class Result : IEquatable<Result>
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        public static Result Success => new Result() { Code = 200 };
        public static Result Disconnected => new Result() {Code = 599};
        public static Result ServerError(string message)
        {
            return new Result() { Code = 500, Description = message };
        }

        public static Result BadRequest(string message)
        {
            return new Result() { Code = 400, Description = message };
        }

        public static Result Unauthorized => new Result() { Code = 403, Description = "unauthorized" };

        public static Result NotFound(string message)
        {
            return new Result() { Code = 404, Description = message };
        }

        public static Result Unauthenticated => new Result() { Code = 407, Description = "Proxy Authentication Required" };

        public static Result PreconditionFailed(string message)
        {
            return new Result() { Code = 412, Description = message };
        }
        public static Result PreconditionRequired(string message)
        {
            return new Result() { Code = 428, Description = message };
        }
        public static Result Conflict(string message)
        {
            return new Result() { Code = 409, Description = message };
        }

        public bool Equals(Result other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return string.Equals(Code, other.Code) && string.Equals(Description, other.Description);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((Result)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Code != null ? Code.GetHashCode() : 0) * 397) ^ (Description != null ? Description.GetHashCode() : 0);
            }
        }
    }
}

