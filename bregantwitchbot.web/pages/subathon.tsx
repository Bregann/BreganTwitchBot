import { Container, Grid, Paper, Table, Text } from '@mantine/core';

const Subathon = () => {
    return (
        <>
            <Text size={60} weight={500} align='center'>Subathon</Text>
            <Text size={30} weight={400} align='center' pt={20}>The subathon will end in...</Text>
            <Text size={30} weight={400} align='center' pt={20}>Today</Text>

            <Text size={35} weight={500} align='center' pt={50} pb={20}>Top 10 Gifters/Cheerers</Text>

            <Container size='xl'>
                <Grid>
                    <Grid.Col span={6}>
                        <Text size={30} weight={400} align='center' pt={20} pb={20}>Gifted</Text>
                        <Paper shadow="md" radius="lg" p="md" sx={(theme) => ({
                            backgroundColor: theme.colors.dark[6]
                        })}>
                            <Table>
                                <thead>
                                    <tr>
                                        <th>Username</th>
                                        <th>Amount</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>Amouranth</td>
                                        <td>500</td>
                                    </tr>
                                </tbody>
                            </Table>
                        </Paper>
                    </Grid.Col>
                    <Grid.Col span={6}>
                        <Text size={30} weight={400} align='center' pt={20} pb={20}>Cheers</Text>
                        <Paper shadow="md" radius="lg" p="md" sx={(theme) => ({
                            backgroundColor: theme.colors.dark[6]
                        })}>
                            <Table>
                                <thead>
                                    <tr>
                                        <th>Username</th>
                                        <th>Amount</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>Amouranth</td>
                                        <td>500</td>
                                    </tr>
                                </tbody>
                            </Table>
                        </Paper>
                    </Grid.Col>
                </Grid>
            </Container>

        </>
    );
}

export default Subathon;