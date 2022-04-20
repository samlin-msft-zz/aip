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
 * @brief Defines GetTemplatesSettings interface
 * 
 * @file get_template_settings.h
 */

#ifndef API_MIP_PROTECTION_GET_TEMPLATE_SETTINGS_H_
#define API_MIP_PROTECTION_GET_TEMPLATE_SETTINGS_H_

#include <memory>
#include <string>

#include "mip/mip_export.h"
#include "mip/mip_namespace.h"
#include "mip/protection/protection_common_settings.h"

MIP_NAMESPACE_BEGIN

/**
 * @brief Settings to use when obtaining templates
 */
class GetTemplatesSettings : public ProtectionCommonSettings {
public:
  /**
   * @brief Sets the delegated user
   *
   * @param delegatedUserEmail the delegation email.
   * 
   * @note A delegated user is specified when the authenticating user/application is acting on behalf of another user
   */
  virtual void SetDelegatedUserEmail(const std::string& delegatedUserEmail) = 0;

  /**
   * @brief Gets the delegated user
   * 
   * @return Delegated user
   * 
   * @note A delegated user is specified when the authenticating user/application is acting on behalf of another user
   */
  virtual const std::string& GetDelegatedUserEmail() const = 0;

  /**
   * @brief Creates a GetTemplatesSettings object
   *
   * @return A GetTemplatesSettings object
   */
  MIP_API static std::shared_ptr<GetTemplatesSettings> __CDECL CreateGetTemplatesSettings();

  /** @cond DOXYGEN_HIDE */
  virtual ~GetTemplatesSettings() {}
protected:
  GetTemplatesSettings() {}
  /** @endcond */
};

MIP_NAMESPACE_END

#endif // API_MIP_PROTECTION_GET_TEMPLATE_SETTINGS_H_
