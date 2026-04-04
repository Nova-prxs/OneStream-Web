---
title: "User Authentication and Single Sign-On"
book: "design-reference-guide"
chapter: 14
start_page: 1769
end_page: 1769
---

# User Authentication And Single

# Sign-On

When you log in to an application, your username and password are authenticated to confirm that you are a valid OneStream user. If you are in a OneStream-hosted environment, see the Identity and Access Management Guide for information about authentication with OneStream IdentityServer. If you are in a self-hosted environment, administrators can configure an environment to use native authentication, one external identity provider, or both native authentication and one external identity provider. The following external identity providers are supported: l Microsoft Active Directory (MSAD) l Lightweight Directory Access Protocol (LDAP) l Three OpenID Connect (OIDC) identity providers: o Azure Active Directory (Azure AD [Microsoft Entra ID]) o Okta o PingFederate l SAML 2.0 identity providers (for example, Okta, PingFederate, Active Directory Federation Services [ADFS], and Salesforce) For instructions on configuring authentication in a self-hosted environment, see the Installation and Configuration Guide.
