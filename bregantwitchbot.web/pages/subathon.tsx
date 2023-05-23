import { DoGet } from '@/helpers/webFetchHelper';
import { Container, Grid, Paper, Table, Text } from '@mantine/core';
import { GetServerSideProps } from 'next';
import { useEffect, useState } from 'react';

interface SubathonProps {
    data: GetSubathonLeaderboardsDto | null;
    timeLeft: GetSubathonTimeLeftDto;
}


interface GetSubathonLeaderboardsDto {
    subsLeaderboard: Leaderboard[];
    bitsLeaderboard: Leaderboard[];
}

interface GetSubathonTimeLeftDto {
    secondsLeft: number;
    playSound: boolean;
    timeUpdated: boolean;
}

export interface Leaderboard {
    username: string;
    subsGifted: number;
    bitsDonated: number;
}

const Subathon = (props: SubathonProps) => {
    const [timeLeft, setTimeLeft] = useState(props.timeLeft);
    const [leaderboardData, setLeaderboardData] = useState(props.data);
    const [secondsLeft, setSecondsLeft] = useState(props.timeLeft.secondsLeft);

    useEffect(() => {
        setInterval(async () => {
            try {
                const timeLeftApiRes = await DoGet('/api/Subathon/GetSubathonTimeLeft');
        
                if (timeLeftApiRes.ok) {
                    const resData: GetSubathonTimeLeftDto = await timeLeftApiRes.json();
                    setTimeLeft(resData);

                    if(resData.timeUpdated){
                        setSecondsLeft(resData.secondsLeft);
                    }
                }
                else {
                    console.error(timeLeftApiRes.statusText);
                    setTimeLeft({
                        secondsLeft: -1,
                        playSound: false,
                        timeUpdated: false
                    });
                }
        
            } catch (error) {
                console.error(error);
                setTimeLeft({
                    secondsLeft: -1,
                    playSound: false,
                    timeUpdated: false
                });
            }
        }, 5000);

        setInterval(async () => {
            try {
                const leaderboardApiRes = await DoGet('/api/Subathon/GetSubathonLeaderboards');

                if (leaderboardApiRes.ok) {
                    setLeaderboardData(await leaderboardApiRes.json());
                }
                else {
                    console.error(leaderboardApiRes.status);
                    setLeaderboardData(null);
                }
            } catch (error) {
                console.error(error);
                setLeaderboardData(null);
            }
        }, 60000);
    }, []);

    useEffect(() => {
        setInterval(() => {
            setSecondsLeft(seconds => seconds - 1);
        }, 1000);
      }, []);

    const convertSecondsToTime = () => {
        const hours = Math.floor(secondsLeft / 3600);
        const minutes = Math.floor((secondsLeft % 3600) / 60);
        const seconds = Math.floor(secondsLeft % 60);
    
        return `${hours} ${hours === 1 ? 'Hour' : 'Hours'} ${minutes} ${hours === 1 ? 'Minute' : 'Minutes'} ${seconds} ${hours === 1 ? 'Second' : 'Seconds'}`;
      };

    return (
        <>
            {timeLeft.playSound &&
                <audio autoPlay>
                    <source src="/quack.mp3" type="audio/mpeg" />
                </audio>
            }

            <Text size={60} weight={500} align='center'>Subathon</Text>
            <Text size={30} weight={400} align='center' pt={20}>The subathon will end in...</Text>
            <Text size={30} weight={400} align='center' pt={20}>{convertSecondsToTime()}</Text>

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
                                    {leaderboardData && leaderboardData.subsLeaderboard.map((user) => (
                                        <tr key={user.username}>
                                            <td>
                                                {user.username}
                                            </td>
                                            <td>
                                                {user.subsGifted}
                                            </td>
                                        </tr>
                                    ))}
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
                                    {leaderboardData && leaderboardData.bitsLeaderboard.map((user) => (
                                        <tr key={user.username}>
                                            <td>
                                                {user.username}
                                            </td>
                                            <td>
                                                {user.bitsDonated}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Paper>
                    </Grid.Col>
                </Grid>
            </Container>

        </>
    );
}

export const getServerSideProps: GetServerSideProps<SubathonProps> = async () => {
    let propsRes: SubathonProps = {
        timeLeft: {
            secondsLeft: -1,
            playSound: false,
            timeUpdated: false
        },
        data: null
    };

    try {
        const timeLeftApiRes = await DoGet('/api/Subathon/GetSubathonTimeLeft');

        if (timeLeftApiRes.ok) {
            propsRes.timeLeft = await timeLeftApiRes.json();
        }
        else {
            console.error(timeLeftApiRes.status);

            propsRes.timeLeft = {
                secondsLeft: -1,
                playSound: false,
                timeUpdated: false
            };
        }

    } catch (error) {
        console.error(error);

        propsRes.timeLeft = {
            secondsLeft: -1,
            playSound: false,
            timeUpdated: false
        };
    }

    try {
        const leaderboardApiRes = await DoGet('/api/Subathon/GetSubathonLeaderboards');

        if (leaderboardApiRes.ok) {
            propsRes.data = await leaderboardApiRes.json();
        }
        else {
            propsRes.data = null;
        }
    } catch (error) {
        propsRes.data = null;
    }

    return {
        props: propsRes
    };
}

export default Subathon;