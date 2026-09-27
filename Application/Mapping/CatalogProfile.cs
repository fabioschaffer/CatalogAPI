using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public class CatalogProfile : Profile
    {
        public CatalogProfile() 
        {
            CreateMap<GameRequest, Game>();
            CreateMap<GameUpdateRequest, Game>();

            CreateMap<LibraryRequest, Library>();
        }
    }
}
