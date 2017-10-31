/*
 * Copyright (c) 2017 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CCM.Core.Cache;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio;
using FakeItEasy;
using LazyCache;
using NUnit.Framework;

namespace CCM.UnitTests.CCM.Core.Cache
{
    [TestFixture]
    public class CachedRegisteredSipRepositoryTests2
    {
        IRegisteredSipRepository _internalRegisteredSipRepository;
        private IAppCache _cache;

        private CachedRegisteredSipRepository _sut;

        [SetUp]
        public void SetUp()
        {
            _internalRegisteredSipRepository = A.Fake<IRegisteredSipRepository>();
            _cache = new CachingService();

            // Clear cache
            _cache.FullReload();

            _sut = new CachedRegisteredSipRepository(_cache, _internalRegisteredSipRepository);
        }

        [Test]
        public void should_load_list_from_internal_repository_on_first_access()
        {
            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void should_not_load_list_from_internal_repository_on_second_access()
        {
            _sut.GetCachedRegisteredSips();
            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void should_reload_list_from_internal_repository_when_codec_added()
        {
            var newRegisteredSip = new RegisteredSip() { SIP = "c" };
            A.CallTo(() => _internalRegisteredSipRepository.UpdateRegisteredSip(newRegisteredSip)).Returns(
                new KamailioMessageHandlerResult()
                {
                    ChangedObjectId = Guid.NewGuid(),
                    ChangeStatus = KamailioMessageChangeStatus.CodecAdded
                });

            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
            _sut.UpdateRegisteredSip(newRegisteredSip);
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Test]
        public void should_not_reload_list_from_internal_repository_when_codec_reregistered()
        {
            var registeredSip = new RegisteredSip() { SIP = "a@acip.example.com" };

            var internalList = new List<RegisteredSipDto>
            {
                new RegisteredSipDto {Id = Guid.NewGuid(), Sip = "a@acip.example.com"}
            };

            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).Returns(internalList);

            A.CallTo(() => _internalRegisteredSipRepository.UpdateRegisteredSip(registeredSip)).Returns(
                new KamailioMessageHandlerResult()
                {
                    ChangedObjectId = internalList[0].Id,
                    ChangeStatus = KamailioMessageChangeStatus.NothingChanged
                });

            // Act
            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
            _sut.UpdateRegisteredSip(registeredSip);
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void should_load_registeredSips_if_list_is_non_existing()
        {
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).Returns(new List<RegisteredSipDto>
            {
                new RegisteredSipDto {Id = Guid.NewGuid(), Sip = "a@acip.example.com"},
                new RegisteredSipDto {Id = Guid.NewGuid(), Sip = "b@acip.example.com"}
            });

            var regSipList = _sut.GetCachedRegisteredSips();

            Assert.IsNotNull(regSipList);
            Assert.AreEqual(2, regSipList.Count);
            Assert.AreEqual("a@acip.example.com", regSipList.First().Sip);

        }

        [Test]
        public void should_not_reload_registeredSips_if_list_already_loaded()
        {
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).Returns(new List<RegisteredSipDto>
            {
                new RegisteredSipDto {Id = Guid.NewGuid(), Sip = "a@acip.example.com"},
                new RegisteredSipDto {Id = Guid.NewGuid(), Sip = "b@acip.example.com"}
            });

            var regSipList = _sut.GetCachedRegisteredSips();
            Assert.IsNotNull(regSipList);
            Assert.AreEqual(2, regSipList.Count);
            Assert.AreEqual("a@acip.example.com", regSipList.First().Sip);
            
            var regSipList2 = _sut.GetCachedRegisteredSips();

            Assert.IsNotNull(regSipList2);
            Assert.AreEqual(2, regSipList2.Count);
            Assert.AreEqual("a@acip.example.com", regSipList2.First().Sip);

            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);

        }

        [Test]
        public void should_reload_registeredSips_when_codec_added()
        {
            var registeredSipDto = new RegisteredSipDto { Id = Guid.NewGuid(), Sip = "a@acip.example.com" };

            var internalList = new List<RegisteredSipDto>();

            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).Returns(internalList);

            A.CallTo(() => _internalRegisteredSipRepository.UpdateRegisteredSip(A<RegisteredSip>.Ignored)).Returns(
                new KamailioMessageHandlerResult()
                {
                    ChangedObjectId = Guid.NewGuid(),
                    ChangeStatus = KamailioMessageChangeStatus.CodecAdded
                });

            // Act
            var regSipList = _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
            Assert.IsNotNull(regSipList);
            Assert.AreEqual(0, regSipList.Count);

            internalList.Add(registeredSipDto);
            _sut.UpdateRegisteredSip(new RegisteredSip() { SIP = "a@acip.example.com" });
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);

            var regSipList2 = _sut.GetCachedRegisteredSips();
            Assert.IsNotNull(regSipList2);
            Assert.AreEqual(1, regSipList2.Count);
            Assert.AreEqual("a@acip.example.com", regSipList2[0].Sip);
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Twice);

        }

        [Test]
        public void should_reload_registeredSips_when_codec_removed()
        {
            // Assign
            var registeredSipDto = new RegisteredSipDto { Id = Guid.NewGuid(), Sip = "a@acip.example.com" };

            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).Returns(new List<RegisteredSipDto> { registeredSipDto });

            A.CallTo(() => _internalRegisteredSipRepository.UpdateRegisteredSip(A<RegisteredSip>.Ignored))
                .Returns( new KamailioMessageHandlerResult { ChangedObjectId = registeredSipDto.Id, ChangeStatus = KamailioMessageChangeStatus.CodecRemoved });

            // Act
            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);
            
            _sut.UpdateRegisteredSip(new RegisteredSip { SIP = "a@acip.example.com" });
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Once);

            _sut.GetCachedRegisteredSips();
            A.CallTo(() => _internalRegisteredSipRepository.GetCachedRegisteredSips()).MustHaveHappened(Repeated.Exactly.Twice);
        }
        
    }
}