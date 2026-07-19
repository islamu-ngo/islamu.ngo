// ABOUTME: Lookup service for retrieving madhab collections and individual madhab records.
// ABOUTME: Keeps Blazor components isolated behind the generated ISLAMU API client contract.

using System.Collections.Generic;
using System.Threading.Tasks;
using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Contracts.Services.Lookup;

namespace Explore.Blazor.Client.Services.Lookup;

public class MadhabService : IMadhabService
{
    private readonly IIslamuApiClient _client;

    public MadhabService(IIslamuApiClient client)
    {
        _client = client;
    }

    public async Task<ICollection<MadhabListDto>> GetMadhabsAsync()
    {
        return await _client.GetMadhabsAsync();
    }

    public async Task<MadhabDto> GetMadhabByIdAsync(int id)
    {
        return await _client.GetMadhabByIdAsync(id);
    }
}
