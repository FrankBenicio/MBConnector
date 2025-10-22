using MBConnector.Data.Dtos;
using MBConnector.Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace MBConnector.Api.Controllers;

[ApiController]
[Route("mercadobitcoin")]
public sealed class MercadoBitcoinController : ControllerBase
{
    private readonly IMercadoBitcoinApiService _apiService;

    public MercadoBitcoinController(IMercadoBitcoinApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Retorna as contas vinculadas à autenticação.
    /// </summary>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(IEnumerable<AccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _apiService.GetAccountsAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// Retorna as posições (open orders) da conta informada.
    /// </summary>
    [HttpGet("accounts/{accountId}/positions")]
    [ProducesResponseType(typeof(IEnumerable<PositionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPositions(
        [FromRoute] string accountId,
        [FromQuery] string? symbols)
    {
        var positions = await _apiService.GetPositionsAsync(accountId, symbols);
        return Ok(positions);
    }
}