using AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Address, AddressDto>();
        CreateMap<City, CityDto>();
        CreateMap<Company, CompanyDto>();
    }
}
