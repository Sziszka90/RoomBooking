using AutoMapper;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Domain;

namespace RoomBooking.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Room, RoomDto>();
        CreateMap<CreateRoomDto, Room>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore());

        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.NumberOfDays, opt => opt.MapFrom(src => src.NumberOfDays));
        CreateMap<CreateBookingDto, Booking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Room, opt => opt.Ignore())
            .ForMember(dest => dest.BookingDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsCancelled, opt => opt.Ignore());
    }
}
