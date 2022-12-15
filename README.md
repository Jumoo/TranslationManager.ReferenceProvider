## Translation Manager - Reference Connector 
(vUmbraco 10/11)

This Repository contains a reference connector for Jumoo's Translation Manager tool for Umbraco. 

**The reference provider shows how to manage the submit,check,cancel processes, but it does not fully function as a translating provider, it requires additional code to connect, translate and process content**

Translation Manager's connector interface allows custom code to be placed into the job creations, check and approval process of Translation Manager so translations can be sent and recevied from third party systems. there are a number of existing connectors - 

https://jumoo.co.uk/translate/providers/

You might want to check to see if one exists before you write a custom one. 

*We (Jumoo) are also avalible to write a connector for your system if you require (£ cost involved) - we have written a number of the existing connectors - so can turn them around quite quickly if needed.*


## Getting Started

The UmbracoSite in the repo, will need to be setup the first time you check the project out. 

- remove the 'ConnectionStrings' element from the appsettings.json and then run the site. 

- This will prompt you to install a new site, the Umbraco Starter kit will be included. 

- Add additional languages to the site and then copy the home content to a new root, 

- this will then allow you to create a translation set on the site, and you can send content between site.s

---

The reference connector contains the basic info to get started building a custom connector for Translation Manager 

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.



