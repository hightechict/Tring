//Copyright 2019 Hightech ICT and authors

//This file is part of Tring.

//Tring is free software: you can redistribute it and/or modify
//it under the terms of the GNU Lesser General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//Tring is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU Lesser General Public License for more details.

//You should have received a copy of the GNU Lesser General Public License
//along with Tring.If not, see<https://www.gnu.org/licenses/>.

using FluentAssertions;
using System;
using System.Net;
using Tring;
using Xunit;

namespace TringTests
{
    public class ConnectionTesterCreation
    {
        [Theory]
        [InlineData("http://google.nl")]
        [InlineData("http://google.nl/test")]
        [InlineData("http://google.nl:80")]
        [InlineData("http://google.nl:80/test")]
        public void CreateConnectionTesterURL(string host)
        {
            var tester = new ConnectionTester(host, false);
            tester.request.Url.Should().Be("google.nl");
            tester.request.Port.Should().Be(80);
        }
        [Theory]
        [InlineData("google.nl:80")]
        [InlineData("google.nl:http")]
        public void CreateConnectionTesterPartialURL(string host)
        {
            var tester = new ConnectionTester(host, false);
            tester.request.Url.Should().Be("google.nl");
            tester.request.Port.Should().Be(80);
        }

        [Theory]
        [InlineData("1.1.1.1:80")]
        [InlineData("1.1.1.1:http")]
        public void CreateConnectionTesterIP(string host)
        {
            var tester = new ConnectionTester(host, false);
            tester.request.Ip.Should().Be(IPAddress.Parse("1.1.1.1"));
            tester.request.Port.Should().Be(80);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1.1.1.1")]
        [InlineData("1.1.1.1:nonsense")]
        public void CreateConnectionTesterIncorrectly(string host)
        {
            Action action = () =>
            {
                _ = new ConnectionTester(host, false);
            };
            action.Should().Throw<Exception>();
        }
    }
}
