﻿/* 
 Copyright (c) 2010, NHIN Direct Project
 All rights reserved.

 Authors:
    Chris Lomonico  chris.lomonico@surescripts.com
 
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the The NHIN Direct Project (nhindirect.org). nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using DnsResolver;
using NHINDirect.Caching;

namespace NHINDirect.Dns
{
    /// <summary>
    /// This class extends the DnsClient providing additional functionality to the client for
    /// utilization of an underlying cache providing a uniform approach to DnsResponse object resolution
    /// </summary>
    public class DnsClientWithCache : DnsClient
    {


        /// <summary>
        /// instance of the dnsresponse cache object used to cache related DnsResponses resolved by
        /// an this class
        /// </summary>
        protected DnsResponseCache m_cache = null;

        /// <summary>
        /// Gets the Cache of the DnsClientWithCache
        /// </summary>
        /// <value></value>
        public DnsResponseCache Cache
        {
            get
            {
                return m_cache;
            }
        }

        /// <summary>
        ///   Creates a DnsClient instance specifying a string representation of the IP address and using the default DNS port.
        /// </summary>
        /// <param name="server">
        /// A string representation of the DNS server IP address.
        /// </param>
        /// <example><c>var client = new DnsClientWithCache("8.8.8.8");</c></example>
        public DnsClientWithCache(string server)
            : base(server)
        {
            Initialize();
        }

        /// <summary>
        ///   Creates a DnsClient instance specifying the DNS port.
        /// </summary>
        /// <param name="server">
        /// A string representation of the DNS server IP address.
        /// </param>
        /// <param name="port">
        /// The port to use for the DNS requests.
        /// </param>
        /// <example><c>var client = new DnsClientWithCache("8.8.8.8", 8888);</c></example>
        public DnsClientWithCache(string server, int port)
            : base(server, port)
        {
            Initialize();
        }

        /// <summary>
        /// Creates a DnsClient instance specifying an IPAddress representation of the IP address.
        /// </summary>
        /// <param name="server">
        /// The IPAddress of the DNS server. A <see cref="IPAddress"/>
        /// </param>
        public DnsClientWithCache(IPAddress server)
            : base(server)
        {
            Initialize();
        }

        /// <summary>
        /// Creates a DnsClient instance specifying an IPEndPoint representation of the IP address and port.
        /// </summary>
        /// <param name="server">
        /// A <see cref="IPEndPoint"/>
        /// </param>
        public DnsClientWithCache(IPEndPoint server)
            : base(server)
        {
            Initialize();
        }

        /// <summary>
        /// Creates a DnsClient instance specifying an IPEndPoint representation of the IP address and port and 
        /// specifying the timeout and buffer size to use.
        /// </summary>
        /// <param name="server">
        /// A <see cref="IPEndPoint"/>
        /// </param>
        /// <param name="timeout">
        /// Timeout value.
        /// </param>
        /// <param name="maxBufferSize">
        /// Maximum buffer size.
        /// </param>
        public DnsClientWithCache(IPEndPoint server
            , TimeSpan timeout
            , int maxBufferSize)
            : base(server, timeout, maxBufferSize)
        {
            Initialize();
        }

        /// <summary>
        /// initializes any object(s) utilized by an instance of this class
        /// </summary>
        /// <remarks>if more is needed here, saves having to update each item in the constructors</remarks>
        protected virtual void Initialize()
        {
            m_cache = new DnsResponseCache();

        }

        /// <summary>
        /// overrides base resolve method to provide cache funcationality at time of resolution
        /// </summary>
        /// <param name="request"></param>
        /// <returns>DnsResponse containing the results either pulled from cache or manually resolved</returns>
        public override DnsResponse Resolve(DnsRequest request)
        {
            //----------------------------------------------------------------------------------------------------
            //---check to see if the item is in the cache
            DnsResponse dr = m_cache.Get(request);
            if (dr != null)
            {
                return dr;
            }
            //----------------------------------------------------------------------------------------------------
            //---item as not found in cache, try to get it from the base class
            dr = base.Resolve(request);
            if (dr != null)
            {
                //----------------------------------------------------------------------------------------------------
                //---if found store in the cache for future use
                m_cache.Put(dr);
            }
            return dr;
        }

        /// <summary>
        /// override base method to use DnsClientWithCache passing in specific name server
        /// </summary>
        /// <param name="domain">The domain name to resolve.</param>
        /// <param name="nameserver">Nameserver to use during resolution</param>
        public override IEnumerable<CertRecord> ResolveCERTFromNameServer(string domain, IPAddress nameserver)
        {
            if (string.IsNullOrEmpty(domain))
            {
                throw new ArgumentException("value was null or empty", "domain");
            }
            if (nameserver == null)
            {
                throw new ArgumentNullException("nameserver");
            }

            using (DnsClientWithCache client = new DnsClientWithCache(nameserver))
            {
                client.UseUDPFirst = false;
                return client.ResolveCERT(domain);
            }
        }

    }
}
