using System.Net;

namespace Lynx.IdentityService.Infrastructure.Tests.EmailServiceTests
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }
        public string? CapturedContent { get; private set; }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;

            if (request.Content != null)
            {
                CapturedContent = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
