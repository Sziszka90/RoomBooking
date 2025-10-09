using AutoMapper;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Models;

namespace RoomBooking.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Room, RoomDto>().ReverseMap();
        CreateMap<CreateRoomDto, Room>();

        CreateMap<Booking, BookingDto>().ReverseMap();
        CreateMap<CreateBookingDto, Booking>();
    }
}
