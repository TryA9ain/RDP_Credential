# 抓取主机保存的RDP凭据

本程序为调用mimikatz抓取主机保存的RDP凭据

##### 注意将mimikatz.exe文件和本文件放在同一个目录下！！！

`用法:  RDP_Credential.exe -u user`

### 建议使用方法：

#### 1、先去`c:\Users\`目录下查看主机所存在的用户目录

![](https://github.com/TryA9ain/RDP_Credential/blob/master/picture/Snipaste_2020-12-24_14-03-14.jpg)

#### 2、依次查询主机上的用户目录下是否存有RDP密码文件

![](https://github.com/TryA9ain/RDP_Credential/blob/master/picture/Snipaste_2020-12-24_14-13-02.jpg)

#### 3、选择你想解密的用户rdp密码文件（以下用Administrator做演示）运行程序即可 （注意将mimikatz.exe文件和本文件放在同一个目录下）

![](https://github.com/TryA9ain/RDP_Credential/blob/master/picture/Snipaste_2020-12-24_14-25-08.jpg)


