using AutoMapper;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Application.Requests;
using CryptoJackpot.Wallet.Application.Responses;

namespace CryptoJackpot.Wallet.Application.Configuration;

public class WalletMappingProfile : Profile
{
    public WalletMappingProfile()
    {
        // Request to Command mappings
        CreateMap<CreateCoinPaymentTransactionRequest, CreateCoinPaymentTransactionCommand>();
        
        // Command to Provider Request mappings
        CreateMap<CreateCoinPaymentTransactionCommand, CreateTransactionRequest>();
        
        // Result to Response mappings
        CreateMap<CreateTransactionResult, CreateCoinPaymentTransactionResponse>();
    }
}
