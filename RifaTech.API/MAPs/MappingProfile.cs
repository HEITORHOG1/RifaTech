using AutoMapper;
using RifaTech.API.Entities;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.MAPs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Cliente, ClienteDTO>();
            CreateMap<ClienteDTO, Cliente>();

            CreateMap<Draw, DrawDTO>();
            CreateMap<DrawDTO, Draw>();

            CreateMap<ExtraNumber, ExtraNumberDTO>();
            CreateMap<ExtraNumberDTO, ExtraNumber>();

            CreateMap<Payment, PaymentDTO>();
            CreateMap<PaymentDTO, Payment>();

            CreateMap<Rifa, RifaDTO>();
            CreateMap<RifaDTO, Rifa>();

            CreateMap<Ticket, TicketDTO>();
            CreateMap<TicketDTO, Ticket>();

            CreateMap<UnpaidRifa, UnpaidRifaDTO>();
            CreateMap<UnpaidRifaDTO, UnpaidRifa>();
        }
    }
}