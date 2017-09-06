using System;
using System.IO;
using System.Threading.Tasks;
using DotNetCqs.Autofac.Storage;
using FluentAssertions;
using Xunit;

namespace DotNetCqs.Autofac.Tests
{
    public class CqsFileStorageIntegrationTests : IDisposable
    {
        public string _path;

        public CqsFileStorageIntegrationTests()
        {
            _path= Path.GetTempFileName();
            _path = _path.Remove(_path.Length - 4, 4);
        }

        public void Dispose()
        {
            Directory.Delete(_path, true);
        }

        [Fact]
        public async Task Store_and_push_a_command()
        {
            var expected = new MyCommand();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected);
            var actual = (MyCommand) await sut.PopCommandAsync();

            actual.TheId.Should().Be(expected.TheId);
        }

        [Fact]
        public async Task Store_and_push_a_query()
        {
            var expected = new MyQuery();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected);
            var actual = (MyQuery) await sut.PopQueryAsync();

            actual.TheId.Should().Be(expected.TheId);
        }

        [Fact]
        public async Task Store_and_push_a_event()
        {
            var expected = new MyEvent();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected);
            var actual = (MyEvent) await sut.PopEventAsync();

            actual.TheId.Should().Be(expected.TheId);
        }

        [Fact]
        public async Task Store_and_push_a_request()
        {
            var expected = new MyRequest();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected);
            var actual = (MyRequest) await sut.PopRequestAsync();

            actual.TheId.Should().Be(expected.TheId);
        }

        [Fact]
        public async Task store_two_Commands_and_get_them_back_in_correct_order()
        {
            var expected1 = new MyCommand();
            var expected2 = new MyCommand();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected1);
            await sut.PushAsync(expected2);
            var actual1 = (MyCommand)await sut.PopCommandAsync();
            var actual2 = (MyCommand)await sut.PopCommandAsync();

            actual1.TheId.Should().Be(expected1.TheId);
            actual2.TheId.Should().Be(expected2.TheId);

        }

        [Fact]
        public async Task store_two_Queries_and_get_them_back_in_correct_order()
        {
            var expected1 = new MyQuery();
            var expected2 = new MyQuery();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected1);
            await sut.PushAsync(expected2);
            var actual1 = (MyQuery)await sut.PopQueryAsync();
            var actual2 = (MyQuery)await sut.PopQueryAsync();

            actual1.TheId.Should().Be(expected1.TheId);
            actual2.TheId.Should().Be(expected2.TheId);
        }

        [Fact]
        public async Task store_two_Events_and_get_them_back_in_correct_order()
        {
            var expected1 = new MyEvent();
            var expected2 = new MyEvent();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected1);
            await sut.PushAsync(expected2);
            var actual1 = (MyEvent)await sut.PopEventAsync();
            var actual2 = (MyEvent)await sut.PopEventAsync();

            actual1.TheId.Should().Be(expected1.TheId);
            actual2.TheId.Should().Be(expected2.TheId);
        }

        [Fact]
        public async Task Store_two_requests_and_get_them_back_in_correct_order()
        {
            var expected1 = new MyRequest();
            var expected2 = new MyRequest();
            var sut = new CqsFileStorage(_path);

            await sut.PushAsync(expected1);
            await sut.PushAsync(expected2);
            var actual1 = (MyRequest)await sut.PopRequestAsync();
            var actual2 = (MyRequest)await sut.PopRequestAsync();

            actual1.TheId.Should().Be(expected1.TheId);
            actual2.TheId.Should().Be(expected2.TheId);
        }


        public class MyCommand : Command
        {
            public MyCommand()
            {
                TheId = Guid.NewGuid();
            }

            public Guid TheId { get; set; }
        }

        public class MyEvent : ApplicationEvent
        {
            public MyEvent()
            {
                TheId = Guid.NewGuid();
            }

            public Guid TheId { get; set; }
        }

        public class MyQuery : Query<string>
        {
            public MyQuery()
            {
                TheId = Guid.NewGuid();
            }

            public Guid TheId { get; set; }
        }

        public class MyRequest : Request<string>
        {
            public MyRequest()
            {
                TheId = Guid.NewGuid();
            }

            public Guid TheId { get; set; }
        }
    }
}