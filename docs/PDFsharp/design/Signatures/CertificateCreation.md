# Certificate creation

This file introduces how OpenSSL can be used to create self-signed certificates that can be used to sign PDF files.

## Creating self-signed certificates

OpenSSL is one option to create certificates for signing.

### Self-signed RSA certificates

For testing, you can easily create an RSA certificate using three calls to OpenSSL:

* Generating a private key
* Generating the self signed certificate
* Create a PFX file containing certificate and private key

Here is code that creates an RSA certificate with a 2048-bits key:

```pwsh
openssl genrsa 2048 > private.pem
openssl req -days 32767 -x509 -new -key private.pem -out public.pem
openssl pkcs12 -export -in public.pem -inkey private.pem -out mycert.pfx 
```

The *-days* parameter specifies how long the certificate will be valid. For some use cases you should not exceed 397 days.

Note that the three statements above will prompt for input like location, company name, and passwords.

### Self-signed DSA certificates

Theoretically, you can create DSA certificates using four calls to OpenSSL. Practically, we did not have success and all DSA certificates we created were rejected by Windows as invalid.

The code is very similar to the RSA case, but specifying the key length requires am extra OpenSSL call to create a parameter file.

```pwsh
openssl dsaparam 2048 > dsaparam.txt
openssl gendsa dsaparam.txt > private.pem
openssl req -days 32767 -x509 -new -key private.pem -out public.pem
openssl pkcs12 -export -in public.pem -inkey private.pem -out mycert.pfx 
```

Please let us know what we were doing wrong if you manage to create DSA certificates using OpenSSL. Thanks in advance.

### Automization of certificate creation

OpenSSL will prompt for information while creating the certificate.
To avoid this, the *subj* parameter can be used:

```txt
-subj "/C=US/ST=Washington/L=Seattle/O=John Doe Ltd./OU=PR/CN=John Doe"
```

Alternatively, you can write the answers into a text file and redirect input to that file.
