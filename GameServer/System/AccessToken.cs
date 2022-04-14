using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using LitJson;

namespace GameServer
{
	public class AccessToken
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member variables

		private Guid m_userId = Guid.Empty;
		private string m_sAccessSecret = null;
		private string m_sSignature = null;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Properties

		public Guid userId
		{
			get { return m_userId; }
		}

		public string accessSecret
		{
			get { return m_sAccessSecret; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public bool Parse(string sAccessToken)
		{
			JsonData token = JsonMapper.ToObject(sAccessToken);
			string sTokenData = null;

			if (!Validate(token, "accessToken", out sTokenData))
				return false;

			string[] tokenValidationParameters = sTokenData.Split('.');

			string sPayload = Encoding.UTF8.GetString(Convert.FromBase64String(tokenValidationParameters[0]));
			JsonData payload = JsonMapper.ToObject(sPayload);

			string sUserId = null;
			if (!Validate(payload, "userId", out sUserId))
				return false;

			m_userId = new Guid(sUserId);

			if (!Validate(payload, "accessSecret", out m_sAccessSecret))
				return false;

			m_sSignature = tokenValidationParameters[1];

			return true;
		}

		private bool Validate(JsonData data, string sKey, out string sParamaters)
		{
			sParamaters = null;

			if (!data.ContainsKey(sKey))
				return false;

			sParamaters = data[sKey].ToString();

			return true;
		}

		public bool Verify()
		{
			JsonData payload = JsonUtil.CreateObject();
			payload["userId"] = m_userId.ToString();
			payload["accessSecret"] = m_sAccessSecret;

			string sPayloadToBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload.ToJson()));

			byte[] securityKey = Encoding.UTF8.GetBytes(AppConfig.accessTokeySecurityKey);
			HMACMD5 hasher = new HMACMD5(securityKey);

			byte[] value = hasher.ComputeHash(Encoding.UTF8.GetBytes(sPayloadToBase64));
			string sSignature = Convert.ToBase64String(value);

			if (m_sSignature != sSignature)
				return false;

			return true;
		}
	}
}
