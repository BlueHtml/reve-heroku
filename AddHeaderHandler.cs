namespace Reve;

public class AddHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Cookie", ReveHelper.Cookie);
        return await base.SendAsync(request, cancellationToken);
    }
}

