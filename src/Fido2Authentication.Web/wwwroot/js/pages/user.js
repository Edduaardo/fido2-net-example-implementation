$(() => {
    $('#user-passkeys-table').DataTable({
        columns: [
            { data: 'name' },
            { data: 'email' },
            {
                data: 'dateRegistered',
                render: (value) => luxon
                    .DateTime
                    .fromISO(value)
                    .toLocaleString(luxon.DateTime.DATETIME_SHORT)
            },
            {
                data: 'id',
                render: () => `<btn type="button" title="Remove" class="btn btn-danger" onclick="deactivateUser(this)"><i class="bi bi-trash"></i></button>`
            }
        ],
        ajax: '/get-users'
    })
})

function deactivateUser() {

}
