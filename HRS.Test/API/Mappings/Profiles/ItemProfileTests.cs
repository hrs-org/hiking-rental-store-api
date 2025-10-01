using AutoMapper;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Mappings.Profiles;
using HRS.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;

namespace HRS.Test.API.Mappings.Profiles;

    public class ItemProfileTests
    {
        private readonly IMapper _mapper;

        public ItemProfileTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => { });

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ItemProfile>();
            }, loggerFactory);

            config.AssertConfigurationIsValid();

            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Should_Map_AddItemRequestDto_To_Item()
        {
            var dto = new AddItemRequestDto
            {
                Name = "Tent",
                Description = "Camping tent",
                Price = 100,
                Quantity = 5,
                Children = new List<AddItemRequestDto>
                {
                    new AddItemRequestDto { Name = "Child Tent", Description = "Small tent", Price = 50, Quantity = 2 }
                }
            };

            var item = _mapper.Map<Item>(dto);

            item.Id.Should().Be(0);
            item.ParentId.Should().BeNull();
            item.Name.Should().Be(dto.Name);
            item.Children.Should().HaveCount(1);
            item.Children.First().Name.Should().Be("Child Tent");
        }

        [Fact]
        public void Should_Map_Item_To_ItemResponseDto()
        {
            var user = new User { Id = 1, FirstName = "Admin", LastName = "User", Email = "r@w.com", Role = Domain.Enums.UserRole.Admin, PasswordHash = "hashedpassword" };
            var item = new Item
            {
                Id = 1,
                Name = "Tent",
                Description = "Camping tent",
                Price = 100,
                Quantity = 5,
                CreatedBy = user,
                Children = new List<Item>
                {
                    new Item { Id = 2, Name = "Child Tent", Description = "Small tent", Price = 50, Quantity = 2, CreatedBy = user }
                }
            };

            var dto = _mapper.Map<ItemResponseDto>(item);

            dto.Id.Should().Be(1);
            dto.Name.Should().Be("Tent");
            dto.Children.Should().HaveCount(1);
            dto.Children.First().Name.Should().Be("Child Tent");
        }
    }

