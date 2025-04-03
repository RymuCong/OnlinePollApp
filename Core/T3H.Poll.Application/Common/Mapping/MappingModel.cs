using AutoMapper;
using T3H.Poll.Application.Polls.DTOs;

namespace T3H.Poll.Application.Common.Mapping
{
    public class MappingModel : Profile
    {
        public MappingModel() 
        {
            MappingEntityToViewModel();
            MappingDtoToEntity();
        }   

        // Get data from Entity map to View Model
        private void MappingEntityToViewModel() 
        {
            // Get data
            CreateMap<Domain.Entities.Poll, PollResponse>();
        }

        private void MappingDtoToEntity()
        {

        }
    }
}
