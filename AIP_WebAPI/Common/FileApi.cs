﻿using Microsoft.InformationProtection;
using Microsoft.InformationProtection.File;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace AIP_WebAPI.Common
{
    public class FileApi
    {

        // This is the location used to store MIP SDK state information and logs.
        private static readonly string mipData = ConfigurationManager.AppSettings["MipData"];
        private readonly string mipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, mipData);

        private readonly ApplicationInfo appInfo;
        private readonly AuthDelegateImplementation _authDelegate;

        private IFileProfile fileProfile;
        private IFileEngine fileEngine;
        private MipContext mipContext;

        /// <summary>
        /// Constructor FileApi object using clientId (from Azure AD), application friendly name, and ClaimsPrincipal representing the user
        /// </summary>
        /// <param name="clientId">Client is the Application ID displayed in the Azure AD App Registration Portal</param>
        /// <param name="applicationName">The application friendly name</param>
        /// <param name="claimsPrincipal">ClaimsPrincipal representing the authenticated user</param>
        public FileApi(string clientId, string applicationName, string applicationVersion, ClaimsPrincipal claimsPrincipal)
        {
            // Store ApplicationInfo and ClaimsPrincipal for SDK operations.
            appInfo = new ApplicationInfo()
            {
                ApplicationId = clientId,
                ApplicationName = applicationName,
                ApplicationVersion = applicationVersion
            };

            // Initialize new AuthDelegate providing the claimsprincipal.
            _authDelegate = new AuthDelegateImplementation(claimsPrincipal);

            // Set path to bins folder.
            var path = Path.Combine(
                  Directory.GetParent(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath)).FullName,
                   Environment.Is64BitProcess ? "bin\\x64" : "bin\\x86");

            // Initialize MIP for File API.  
            MIP.Initialize(MipComponent.File, path);

            // Call CreateFileProfile. Result is stored in global.
            CreateFileProfile();

            // Call CreateFileEngine, providing the user UPN, null client data, and locale.
            CreateFileEngine(ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn).Value, "", "en-US");
        }

        ~FileApi()
        {
            fileEngine = null;
            fileProfile = null;
            mipContext = null;
        }

        /// <summary>
        /// Creates a new IFileProfile object and stores in private _fileProfile.
        /// </summary>
        private void CreateFileProfile()
        {
            try
            {
                mipContext = MIP.CreateMipContext(appInfo, mipPath, LogLevel.Error, null, null);
                var profileSettings = new FileProfileSettings(mipContext, CacheStorageType.OnDisk, new ConsentDelegateImplementation());
                fileProfile = Task.Run(async () => await MIP.LoadFileProfileAsync(profileSettings)).Result;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Initializes a new IFileEngine using the provided username, custom client data string, and locale.
        /// Stores result in _fileEngine.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="clientData"></param>
        /// <param name="locale"></param>
        private void CreateFileEngine(string username, string clientData, string locale)
        {
            try
            {
                Identity id = new Identity(username);
                var engineSettings = new FileEngineSettings(username, _authDelegate, clientData, locale)
                {
                    Identity = id
                };

                fileEngine = Task.Run(async () => await fileProfile.AddEngineAsync(engineSettings)).Result;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Creates an IFileHandler for the specified file or stream.
        /// </summary>
        /// <param name="stream">Can be null. If null, fileName must be the full path to the file.</param>
        /// <param name="fileName">File name or full path. Should be full path is stream is null</param>
        /// <returns>IFileHandler</returns>
        private IFileHandler CreateFileHandler(Stream stream, string fileName)
        {
            IFileHandler handler;

            try
            {
                if (stream != null)
                    handler = Task.Run(async () => await fileEngine.CreateFileHandlerAsync(stream, fileName, true)).Result;
                else
                    handler = Task.Run(async () => await fileEngine.CreateFileHandlerAsync(fileName, fileName, true)).Result;

                return handler;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Applies the specified label to the provided file or stream.
        /// Justification message may be required if downgrading or removing label.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="outputStream"></param>
        /// <param name="fileName"></param>
        /// <param name="labelId"></param>
        /// <param name="justificationMessage"></param>
        /// <returns></returns>
        public ContentLabel GetFileLabel(Stream stream, Stream outputStream, string fileName, out string message)
        {
            ContentLabel contentLabel = null;
            IFileHandler handler;
            message = string.Empty;

            try
            {
                // Try to create an IFileHandler using private CreateFileHandler().
                if (stream != null)
                {
                    handler = CreateFileHandler(stream, fileName);
                }

                // Try to create an IFileHandler using private CreateFileHandler().
                else
                {
                    handler = CreateFileHandler(null, fileName);
                }

                // Applying a label requires LabelingOptions. Hard coded values here, but could be provided by user. 
                LabelingOptions labelingOptions = new LabelingOptions()
                {
                    AssignmentMethod = AssignmentMethod.Standard
                };

                if (null != handler.Label)
                {
                    contentLabel = handler.Label;
                }
                else
                {
                    message = $"The file has no label.";
                }
                handler.Dispose();                
                return contentLabel;
            }
            catch (Exception ex)
            {
                message = $"Failed to get file:{fileName} label information, Error: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Applies the specified label to the provided file or stream.
        /// Justification message may be required if downgrading or removing label.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="outputStream"></param>
        /// <param name="fileName"></param>
        /// <param name="labelId"></param>
        /// <param name="justificationMessage"></param>
        /// <returns></returns>
        public bool ApplyLabel(Stream stream, Stream outputStream, string fileName, string labelId, out string message)
        {
            message = string.Empty;
            bool result = false;

            IFileHandler handler;

            try
            {
                // Try to create an IFileHandler using private CreateFileHandler().
                if (stream != null)
                {
                    handler = CreateFileHandler(stream, fileName);
                }

                // Try to create an IFileHandler using private CreateFileHandler().
                else
                {
                    handler = CreateFileHandler(null, fileName);
                }

                // Applying a label requires LabelingOptions. Hard coded values here, but could be provided by user. 
                LabelingOptions labelingOptions = new LabelingOptions()
                {
                    AssignmentMethod = AssignmentMethod.Standard
                };

                // Set the label on the input stream or file.
                handler.SetLabel(fileEngine.GetLabelById(labelId), labelingOptions, new ProtectionSettings());

                // Call CommitAsync to write result to output stream. 
                // Returns a bool to indicate true or false.
                result = Task.Run(async () => await handler.CommitAsync(outputStream)).Result;

                if (result)
                {
                    // Submit an audit event if the change was successful.
                    handler.NotifyCommitSuccessful(fileName);
                }
                else
                {
                    message = $"Failed to apply label:{labelId}";
                }
                handler.Dispose();
            }
            catch (Exception ex)
            {
                message = $"Failed to apply label:{labelId}\n{ex.Message}\n{ex.InnerException.Message}";
            }
            return result;
        }

        public bool RemoveLabel(Stream stream, Stream outputStream, string fileName,  string justificationMessage, out string message)
        {
            message = string.Empty;
            bool result = false;
            IFileHandler handler;

            try
            {
                // Try to create an IFileHandler using private CreateFileHandler().
                if (stream != null)
                {
                    handler = CreateFileHandler(stream, fileName);
                }

                // Try to create an IFileHandler using private CreateFileHandler().
                else
                {
                    handler = CreateFileHandler(null, fileName);
                }

                // Applying a label requires LabelingOptions. Hard coded values here, but could be provided by user. 
                LabelingOptions labelingOptions = new LabelingOptions()
                {                   
                    AssignmentMethod = AssignmentMethod.Standard
                    //ExtendedProperties = new List<KeyValuePair<string, string>>()
                };

                labelingOptions.IsDowngradeJustified = true;
                labelingOptions.JustificationMessage = string.IsNullOrEmpty(justificationMessage) ? "Label modified by App." : justificationMessage;

                // Set the label on the input stream or file.
                //handler.SetLabel(fileEngine.GetLabelById(labelId), labelingOptions, new ProtectionSettings());
                handler.DeleteLabel(labelingOptions);

                // Call CommitAsync to write result to output stream. 
                // Returns a bool to indicate true or false.
                result = Task.Run(async () => await handler.CommitAsync(outputStream)).Result;

                if (result)
                {
                    // Submit an audit event if the change was successful.
                    handler.NotifyCommitSuccessful(fileName);
                }
                handler.Dispose();                
            }
            catch (Exception ex)
            {
                message = $"Failed to remove label:{ex.Message}\n{ex.InnerException.Message}";
            }
            return result;
        }

        // For custom defined label
        public bool ApplyLabel(Stream stream, Stream outputStream, string fileName, string labelId, string justificationMessage, bool isCustom, ProtectionDescriptor adHocProtectionDescriptor, out string message)
        {
            message = string.Empty;
            bool result = false;

            IFileHandler handler;

            try
            {
                // Try to create an IFileHandler using private CreateFileHandler().
                if (stream != null)
                {
                    handler = CreateFileHandler(stream, fileName);
                }

                // Try to create an IFileHandler using private CreateFileHandler().
                else
                {
                    handler = CreateFileHandler(null, fileName);
                }

                if (isCustom)
                {
                    ProtectionSettings settings = new ProtectionSettings();
                    handler.SetProtection(adHocProtectionDescriptor, settings);
                }

                // Applying a label requires LabelingOptions. Hard coded values here, but could be provided by user. 
                LabelingOptions labelingOptions = new LabelingOptions()
                {
                    JustificationMessage = justificationMessage,
                    AssignmentMethod = AssignmentMethod.Standard,
                    ExtendedProperties = new List<KeyValuePair<string, string>>()
                };

                // Set the label on the input stream or file.
                handler.SetLabel(fileEngine.GetLabelById(labelId), labelingOptions, new ProtectionSettings());

                // Call CommitAsync to write result to output stream. 
                // Returns a bool to indicate true or false.
                result = Task.Run(async () => await handler.CommitAsync(outputStream)).Result;

                if (result)
                {
                    // Submit an audit event if the change was successful.
                    handler.NotifyCommitSuccessful(fileName);
                }
                else
                {
                    message = $"Failed to apply label:{labelId}";
                }
            }
            catch (Exception ex)
            {
                message = $"Failed to apply label:{labelId}\n{ex.Message}";
            }
                return result;
        }

        /// <summary>
        /// Calls _fileEngine to list all available labels, iterates through list and puts in List<Models.Label>
        /// </summary>
        /// <returns>List<Models.Label></returns>
        public List<Models.Label> ListAllLabels()
        {
            try
            {
                var labels = fileEngine.SensitivityLabels;
                var returnLabels = new List<Models.Label>();

                foreach (var label in labels)
                {
                    var _label = new Models.Label()
                    {
                        Name = label.Name,
                        Id = label.Id,
                        Description = label.Description,
                        Sensitivity = label.Sensitivity
                    };

                    _label.Children = new List<Models.Label>();

                    // If the label has an children, iterate through each. 
                    if (label.Children.Count > 0)
                    {
                        foreach (var child in label.Children)
                        {
                            var _child = new Models.Label()
                            {
                                Name = child.Name,
                                Id = child.Id,
                                Description = child.Description,
                                Sensitivity = child.Sensitivity
                            };
                            _label.Children.Add(_child);
                        }
                    }
                    returnLabels.Add(_label);
                }

                return returnLabels;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}