﻿using ThoughtWorks.CruiseControl.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    using NUnit.Framework;
    using ThoughtWorks.CruiseControl.Remote;
    using System.Collections.Specialized;
    using System;
    using System.Net;
    using ThoughtWorks.CruiseControl.Remote.Messages;
    using System.Text;
    using NUnit.Framework.Legacy;

    [TestFixture]
    public class HttpConnectionTests
    {
        #region Tests
        #region IsBusy tests
        [Test]
        public void IsBusyReturnsFalseWhenNothingIsHappening()
        {
            var connection = new HttpConnection("http://somewhere");
            ClassicAssert.IsFalse(connection.IsBusy);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }
        #endregion

        #region Address tests
        [Test]
        public void AddressReturnsTheUrl()
        {
            var url = "http://somewhere/";
            var connection = new HttpConnection(url);
            ClassicAssert.AreEqual(url, connection.Address);
        }
        #endregion

        #region SendMessage() tests
        [Test]
        public void SendMessageSendsAndReceivesAMessage()
        {
            var action = "DoSomething";
            var request = new ServerRequest
            {
                ServerName = "TestServer"
            };
            var url = "http://somewhere/";
            var factory = new TestClientFactory((u, a, d) =>
            {
                ClassicAssert.AreEqual(url + "server/TestServer/RawXmlMessage.aspx", u.AbsoluteUri);
                ClassicAssert.AreEqual("POST", a);
                ClassicAssert.AreEqual(action, d["action"]);
                ClassicAssert.AreEqual(request.ToString(), d["message"]);
                var theResponse = new Response
                {
                    RequestIdentifier = request.Identifier
                };
                return Encoding.UTF8.GetBytes(theResponse.ToString());
            });
            var connection = new HttpConnection(new Uri(url), factory);
            var response = connection.SendMessage(action, request);
            ClassicAssert.IsInstanceOf<Response>(response);
            ClassicAssert.AreEqual(request.Identifier, response.RequestIdentifier);
        }
        #endregion

        #region
        [Test]
        public void SendMessageAsyncSendsMessage()
        {
            var action = "DoSomething";
            var request = new ServerRequest
            {
                ServerName = "TestServer"
            };
            var url = "http://somewhere/";
            var factory = new TestClientFactory((u, a, d) =>
            {
                ClassicAssert.AreEqual(url + "server/TestServer/RawXmlMessage.aspx", u.AbsoluteUri);
                ClassicAssert.AreEqual("POST", a);
                ClassicAssert.AreEqual(action, d["action"]);
                ClassicAssert.AreEqual(request.ToString(), d["message"]);
                var theResponse = new Response
                {
                    RequestIdentifier = request.Identifier
                };
                return Encoding.UTF8.GetBytes(theResponse.ToString());
            });
            var connection = new HttpConnection(new Uri(url), factory);
            var completed = false;
            connection.SendMessageCompleted += (o, e) =>
            {
                completed = true;
                ClassicAssert.IsFalse(e.Cancelled);
                ClassicAssert.IsNull(e.Error);
            };
            connection.SendMessageAsync(action, request);
            ClassicAssert.IsTrue(completed);
        }

        [Test]
        public void SendMessageAsyncPassesOnRemoteException()
        {
            var action = "DoSomething";
            var request = new ServerRequest
            {
                ServerName = "TestServer"
            };
            var url = "http://somewhere/";
            var errorMessage = "Oops, an error happened";
            var factory = new TestClientFactory((u, a, d) =>
            {
                throw new CruiseControlException(errorMessage);
            });
            var connection = new HttpConnection(new Uri(url), factory);
            var completed = false;
            connection.SendMessageCompleted += (o, e) =>
            {
                completed = true;
                ClassicAssert.IsFalse(e.Cancelled);
                ClassicAssert.IsNotNull(e.Error);
                ClassicAssert.AreEqual(errorMessage, e.Error.Message);
                ClassicAssert.IsNull(e.Response);
            };
            connection.SendMessageAsync(action, request);
            ClassicAssert.IsTrue(completed);
        }

        [Test]
        public void SendMessageAsyncPassesOnLocalException()
        {
            var action = "DoSomething";
            var request = new ServerRequest
            {
                ServerName = "TestServer"
            };
            var url = "http://error";
            var errorMessage = "Oops, an error happened";
            var factory = new TestClientFactory((u, a, d) =>
            {
                throw new CruiseControlException(errorMessage);
            });
            var connection = new HttpConnection(new Uri(url), factory);
            var completed = false;
            connection.SendMessageCompleted += (o, e) =>
            {
                completed = true;
                ClassicAssert.IsFalse(e.Cancelled);
                ClassicAssert.IsNotNull(e.Error);
                ClassicAssert.AreEqual("Oops, unknown address", e.Error.Message);
                ClassicAssert.IsNull(e.Response);
            };
            connection.SendMessageAsync(action, request);
            ClassicAssert.IsTrue(completed);
        }

        [Test]
        public void SendMessageAsyncCanBeCancelled()
        {
            var action = "DoSomething";
            var request = new ServerRequest
            {
                ServerName = "TestServer"
            };
            var url = "http://nowhere";
            HttpConnection connection = null;
            var factory = new TestClientFactory((u, a, d) =>
            {
                connection.CancelAsync();
                return new byte[0];
            });
            connection = new HttpConnection(new Uri(url), factory);
            var completed = false;
            connection.SendMessageCompleted += (o, e) =>
            {
                completed = true;
                ClassicAssert.IsTrue(e.Cancelled);
                ClassicAssert.IsNull(e.Error);
                ClassicAssert.IsNull(e.Response);
            };
            connection.SendMessageAsync(action, request);
            ClassicAssert.IsTrue(completed);
        }
        #endregion
        #endregion

        #region Private classes
        #region TestClientFactory
        private class TestClientFactory
            : IWebClientFactory
        {
            private Func<Uri, string, NameValueCollection, byte[]> action;

            public TestClientFactory(Func<Uri, string, NameValueCollection, byte[]> action)
            {
                this.action = action;
            }

            public IWebClient Generate()
            {
                return new TestClient(this.action);
            }
        }
        #endregion

        #region TestClient
        private class TestClient
            : IWebClient
        {
            private Func<Uri, string, NameValueCollection, byte[]> action;
            private bool cancelled = false;

            public TestClient(Func<Uri, string, NameValueCollection, byte[]> action)
            {
                this.action = action;
            }

            public byte[] UploadValues(Uri address, string method, NameValueCollection data)
            {
                return action(address, method, data);
            }

            public void UploadValuesAsync(Uri address, string method, NameValueCollection data)
            {
                if (address.AbsoluteUri.StartsWith("http://error"))
                {
                    throw new CruiseControlException("Oops, unknown address");
                }
                else
                {
                    try
                    {
                        var binaryData = this.action(address, method, data);
                        if (this.UploadValuesCompleted != null)
                        {
                            var args = new BinaryDataEventArgs(binaryData, null, cancelled, null);
                            this.UploadValuesCompleted(this, args);
                        }
                    }
                    catch (Exception error)
                    {
                        var args = new BinaryDataEventArgs(new byte[0], error, cancelled, null);
                        this.UploadValuesCompleted(this, args);
                    }
                }
            }

            public void CancelAsync()
            {
                this.cancelled = true;
            }

            public event EventHandler<BinaryDataEventArgs> UploadValuesCompleted;
        }
        #endregion
        #endregion
    }
}
