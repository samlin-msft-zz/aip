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
 * @brief File containing definition of MipContext
 * 
 * @file mip_context.h
 */

#ifndef API_MIP_MIP_CONTEXT_H_
#define API_MIP_MIP_CONTEXT_H_

#include <map>
#include <memory>

#include "mip/common_types.h"
#include "mip/diagnostic_configuration.h"
#include "mip/flighting_feature.h"
#include "mip/logger_delegate.h"
#include "mip/mip_export.h"
#include "mip/mip_namespace.h"
#include "mip/telemetry_delegate.h"

MIP_NAMESPACE_BEGIN

/** @cond DOXYGEN_HIDE */
class AuditManager;
class ProtectionCloudManager;
class PolicyCloudManager;
class TelemetryManager;
/** @endcond */

/**
 * @brief MipContext represents state that is shared across all profiles, engines, handlers
 */
class MipContext {
public:
  /**
   * @brief Create a new MipContext instance to be used when initializing profiles
   * 
   * @param appInfo Description of host application
   * @param path File path for logs, caches, etc.
   * @param thresholdLogLevel Minimum log level for .miplog
   * @param isOfflineOnly Enable/disable network operations (not all actions supported when offline)
   * @param loggerDelegateOverride (Optional) LoggerDelegate override implementation
   * @param diagnosticOverride (Optional) DiagnosticConfiguration override for audit/telemetry
   * 
   * @return MipContext instance
   */
   MIP_API static std::shared_ptr<MipContext> __CDECL Create(
      const ApplicationInfo& appInfo,
      const std::string& path,
      LogLevel thresholdLogLevel,
      bool isOfflineOnly,
      const std::shared_ptr<LoggerDelegate>& loggerDelegateOverride,
      const std::shared_ptr<DiagnosticConfiguration>& diagnosticOverride);

  /**
   * @brief Create a new MipContext instance with custom feature settings
   * 
   * @param appInfo Description of host application
   * @param path File path for logs, caches, etc.
   * @param thresholdLogLevel Minimum log level for .miplog
   * @param isOfflineOnly Enable/disable network operations (not all actions supported when offline)
   * @param loggerDelegateOverride (Optional) LoggerDelegate override implementation
   * @param diagnosticOverride (Optional) TDiagnosticConfiguration override for audit/telemetry
   * @param featureSettingsOverride Flighting features which should be set to non-default values
   * 
   * @return MipContext instance
   */
  MIP_API static std::shared_ptr<MipContext> __CDECL CreateWithCustomFeatureSettings(
      const ApplicationInfo& appInfo,
      const std::string& path,
      LogLevel thresholdLogLevel,
      bool isOfflineOnly,
      const std::shared_ptr<LoggerDelegate>& loggerDelegateOverride,
      const std::shared_ptr<DiagnosticConfiguration>& diagnosticOverride,
      const std::map<FlightingFeature, bool>& featureSettingsOverride);

  /**
   * @brief Terminate MIP
   * 
   * @note This method must be called prior to process/DLL shutdown
   */
  virtual void ShutDown() = 0;

  /**
   * @brief Gets whether or not a feature is enabled
   * 
   * @param feature Feature to enable/disable
   * 
   * @return Whether or not a feature is enabled
   * 
   * @note If a FeatureFlightingDelegate has not been provided by an application, this will always return true
   */
  virtual bool IsFeatureEnabled(FlightingFeature feature) const = 0;

  /**
   * @brief Get application description
   * 
   * @return Application description
   */
  virtual const ApplicationInfo& GetApplicationInfo() const = 0;

  /**
   * @brief Get file path for logs, caches, etc.
   * 
   * @return File path (with "mip" leaf directory)
   */
  virtual const std::string& GetMipPath() const = 0;

  /**
   * @brief Get offline-only setting
   * 
   * @return Whether or not application is running in offline-only mode
   */
  virtual bool IsOfflineOnly() = 0;
  
  /**
   * @brief Get threshold log level
   * 
   * @return threshold log level
   */
  virtual LogLevel GetThresholdLogLevel() const = 0;

  /**
   * @brief Get logger implementation
   * 
   * @return Logger
   */
  virtual std::shared_ptr<LoggerDelegate> GetLoggerDelegate() = 0;

  /**
   * @brief Get logger implementation
   * 
   * @return Logger
   */
  virtual LoggerDelegate* GetRawLoggerDelegate() = 0;

  /**
   * @brief Get flighting feature set.
   * 
   * @return Flighting feature map
   */
  virtual const std::map<FlightingFeature, bool>& GetFlightingFeatures() const = 0;

  /** @cond DOXYGEN_HIDE */
  virtual const ProtectionCloudManager& GetProtectionCloudManager() const = 0;
  virtual const PolicyCloudManager& GetPolicyCloudManager() const = 0;
  virtual std::shared_ptr<TelemetryManager> GetTelemetryManager() = 0;
  virtual std::shared_ptr<AuditManager> GetAuditManager() = 0;
  virtual TelemetryManager* GetRawTelemetryManager() = 0;
  virtual ~MipContext() {}
  /** @endcond */
};

MIP_NAMESPACE_END
#endif  // API_MIP_MIP_CONTEXT_H_

