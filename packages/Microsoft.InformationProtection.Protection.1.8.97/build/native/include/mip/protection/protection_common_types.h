/*
 *
 * Copyright (c) Microsoft Corporation.
 * All rights reserved.
 *
 * This code is licensed under the MIT License.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files(the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions :
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */
/**
 * @brief A file containing the common types used by the protection module.
 * 
 * @file protection_common_types.h
 */

#ifndef API_MIP_PROTECTION_PROTECTION_COMMON_TYPES_H_
#define API_MIP_PROTECTION_PROTECTION_COMMON_TYPES_H_

#include <map>
#include <string>
#include <vector>
#include "mip/mip_namespace.h"


MIP_NAMESPACE_BEGIN

/**
 * @brief Holds the details of a Publishing License used to create a protection handler
 */
class PublishingLicenseInfo {
public:
  PublishingLicenseInfo(
      const std::vector<uint8_t>& serializedPublishingLicense)
      : mSerializedPublishingLicense(serializedPublishingLicense),
        mIsParsed(false) {}
  
  PublishingLicenseInfo(
      const std::vector<uint8_t>& serializedPreLicense,
      const std::vector<uint8_t>& serializedPublishingLicense)
      : mPreLicense(serializedPreLicense),
        mSerializedPublishingLicense(serializedPublishingLicense),
        mIsParsed(false) {}

  void SetParsedData(
      const std::vector<std::string>& domains,
      const std::string& serverPublicCert,
      const std::string& contentId,
      const std::string& issuerId) {
    mDomains = domains;
    mServerPublicCert = serverPublicCert;
    mContentId = contentId;
    mIssuerId = issuerId;
    mIsParsed = true;
  }

  void SetDoubleKeyData(
      const std::string& algorithm,
      const std::map<std::string, std::string>& doubleKeyApplicationData) {
    mDoubleKeyAlgorithm = algorithm;
    mDoubleKeyApplicationData = doubleKeyApplicationData;
  }

  const std::vector<uint8_t>& GetSerializedPublishingLicense() const { return mSerializedPublishingLicense; }
  const std::vector<uint8_t>& GetPreLicense() const { return mPreLicense; }
  const std::vector<std::string>& GetDomains() const { return mDomains; }
  const std::string& GetServerPublicCertificate() const { return mServerPublicCert; }
  const std::string& GetIssuerId() const { return mIssuerId; }
  const std::string& GetContentId() const { return mContentId; }
  bool IsLicenseParsed() const { return mIsParsed; }
  bool HasPreLicense() const { return !mPreLicense.empty(); }
  bool GetIsDoubleKeyLicense() const { return !mDoubleKeyAlgorithm.empty(); }
  const std::string& GetDoubleKeyAlgorithm() const { return mDoubleKeyAlgorithm; }
  const std::map<std::string, std::string>& GetDoubleKeyApplicationData() const { return mDoubleKeyApplicationData; }
private:
  std::vector<uint8_t> mPreLicense;
  std::vector<uint8_t> mSerializedPublishingLicense;
  std::vector<std::string> mDomains;
  std::string mServerPublicCert;
  std::string mContentId;
  std::string mIssuerId;
  bool mIsParsed;
  std::string mDoubleKeyAlgorithm;
  std::map<std::string, std::string> mDoubleKeyApplicationData;
};

/**
 * @brief Cipher mode identifier
 */
enum class CipherMode {
  CIPHER_MODE_CBC4K = 0,              /**< CBC 4K mode with internal padding */
  CIPHER_MODE_ECB = 1,                /**< ECB mode */
  CIPHER_MODE_CBC512NOPADDING = 2,    /**< CBC 512 mode with external (client) padding */
  CIPHER_MODE_CBC4KNOPADDING = 3      /**< CBC 4K mode with external (client) padding */
};

MIP_NAMESPACE_END

#endif // API_MIP_PROTECTION_PROTECTION_COMMON_TYPES_H_