using AutoMapper;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Domain;

namespace RoomBooking.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Room, RoomResponse>();
        CreateMap<CreateRoomRequest, Room>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore());

        CreateMap<Booking, BookingResponse>()
            .ForMember(dest => dest.NumberOfDays, opt => opt.MapFrom(src => src.NumberOfDays));
        CreateMap<CreateBookingRequest, Booking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Room, opt => opt.Ignore())
            .ForMember(dest => dest.BookingDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsCancelled, opt => opt.Ignore());
    }
}
