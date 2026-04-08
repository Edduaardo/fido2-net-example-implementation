$(() => {
    $('#login-with-passkey').on('click', async () => {
        const login = $('#login').val()
        
        if (!$('#login-form').validate().element('#login'))
            return
        
        const assertionOptions = await fetchAssetionOptions(login)

        assertionOptions.challenge = coerceToArrayBuffer(assertionOptions.challenge)
        assertionOptions.allowCredentials.forEach((listItem) => {
            listItem.id = coerceToArrayBuffer(listItem.id);
        })

        const credential = await navigator.credentials.get({
            publicKey: assertionOptions
        })

        credential.userHandle = login
        
        let assertionResult
        await assertCredential(credential)
            .then((response) => {
                assertionResult = response
            }
        )

        if (assertionResult.status === 200) window.location = '/Home/Index'
        else {
            Swal.fire({
                title: 'Authentication failed',
                text: 'Authentication failed, try again later.',
                //imageUrl: "/images/securitykey.min.svg",
                showCancelButton: true,
                showConfirmButton: false,
                focusConfirm: false,
                focusCancel: false
            })
        }
    })

    const validationSettings = $.data($('#login-form')[0], 'validator').settings
    validationSettings.onfocusout = null
    validationSettings.onkeyup = null

    $('#login-form').bind('invalid-form.validate', () => {
        $("#change-password-form").addClass('was-validated')
    })
})

function fetchAssetionOptions(login) {
    return $.ajax({
        url: '/get-passkey-assertion-options',
        data: {
            login: login
        }
    })
}

function assertCredential(credential) {
    return $.ajax({
        url: '/make-passkey-assertion',
        method: 'POST',
        data: JSON.stringify({
            id: credential.id,
            rawId: coerceToBase64Url(new Uint8Array(credential.rawId)),
            type: credential.type,
            extensions: credential.getClientExtensionResults(),
            response: {
                authenticatorData: coerceToBase64Url(new Uint8Array(credential.response.authenticatorData)),
                clientDataJSON: coerceToBase64Url(new Uint8Array(credential.response.clientDataJSON)),
                signature: coerceToBase64Url(new Uint8Array(credential.response.signature)),
                userHandle: coerceToBase64Url(coerceToArrayBuffer(btoa(credential.userHandle)))
            }
        }),
        dataType: 'json',
        contentType: 'application/json'
    })
}
