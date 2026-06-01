using Microsoft.AspNetCore.Http;
using System.Threading;
using Bridge_App.Models;

namespace Bridge_App.Utilities
{
    public static class GlobalSessionContext
    {
        private static readonly AsyncLocal<SessionData> _session = new AsyncLocal<SessionData>();

        public static SessionData Current
        {
            get
            {
                var httpContext = new HttpContextAccessor().HttpContext;
                if (httpContext != null && httpContext.Items.ContainsKey("Session"))
                {
                    return httpContext.Items["Session"] as SessionData;
                }
                return _session.Value;
            }
            set
            {
                var httpContext = new HttpContextAccessor().HttpContext;
                if (httpContext != null)
                    httpContext.Items["Session"] = value;
                _session.Value = value;
            }
        }

        public static void Clear()
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            if (httpContext != null)
                httpContext.Items.Remove("Session");
            _session.Value = null;
        }
    }
}
