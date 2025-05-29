using AutoMapper;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Entities;

namespace PersonManagement.Application.Mappings;

public class PessoaMappingProfile : Profile
{
    public PessoaMappingProfile()
    {
        CreateMap<Pessoa, PessoaDto>()
            .ForMember(dest => dest.NomeCompleto, opt => opt.MapFrom(src => src.ObterNomeCompleto()))
            .ForMember(dest => dest.Idade, opt => opt.MapFrom(src => src.ObterIdade()));

        CreateMap<PessoaDto, Pessoa>()
            .ConstructUsing(src => new Pessoa(src.Nome, src.Sobrenome, src.Email, src.DataNascimento, src.Documento));
    }
}